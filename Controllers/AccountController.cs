using DACN.Data;
using DACN.DTOs.Request;
using DACN.Models;
using DACN.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static DACN.Enums.StatusEnums;
namespace WebApplication2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmployeeRepository employeeRepository;
        public AccountController(ApplicationDbContext db,EmployeeRepository employeeRepository)
        {
            this.db = db;
            this.employeeRepository = employeeRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            // Kiểm tra trùng username/email
            var existingUser = await db.UserAccounts
                .FirstOrDefaultAsync(u => u.FullName == request.FullName || u.Email == request.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc email đã tồn tại!");
                return View(request);
            }

            var newUser = new UserAccountModel
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.Now,
                Role = UserRole.Employee,
            };

            db.UserAccounts.Add(newUser);
            await db.SaveChangesAsync(); // ✅ Lưu bất đồng bộ
            return RedirectToAction("Login");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(request);
            var user = db.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng!");
                return View(request);
            }
            var employee = db.Employees.FirstOrDefault(e => e.UserAccountId == user.UserAccountId);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserAccountId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),      
            };
            if (employee != null)
            {
                claims.Add(new Claim("EmployeeId", employee.EmployeeId.ToString()));
            }
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateAccount(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("Error", "Link kích hoạt không hợp lệ.");
            }

            // 1. Tìm user qua Repo
            var user = await employeeRepository.GetByActivationTokenAsync(token);

            // 2. Kiểm tra
            if (user == null)
            {
                return View("Error", "Link kích hoạt không tồn tại.");
            }

            if (user.TokenExpiry < DateTime.Now)
            {
                return View("Error", "Link kích hoạt đã hết hạn. Vui lòng liên hệ HR để cấp lại.");
            }

            // 3. Hiển thị Form
            var model = new ActivateAccountRequest { Token = token, Email = user.Email };
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAccount(ActivateAccountRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Tìm lại user qua Repo
            var user = await employeeRepository.GetByActivationTokenAsync(model.Token);

            if (user == null || user.TokenExpiry < DateTime.Now)
            {
                return View("Error", "Link không hợp lệ hoặc đã hết hạn.");
            }

            // 2. Cập nhật thông tin (Logic nghiệp vụ)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.IsActive = true;           // Kích hoạt
            user.ActivationToken = null;    // Xóa token
            user.TokenExpiry = null;        // Xóa hạn dùng
            user.UpdatedAt = DateTime.Now;
            // 3. Lưu vào DB qua Repo
            await employeeRepository.UpdateAsync(user);

            // 4. Chuyển hướng
            ViewBag.IsSuccess = true;
            ViewBag.RedirectUrl = Url.Action("Login", "Account"); // Link tới trang Login

            return View(model); // Trả về lại trang hiện tại để hiện thông báo
        }

    }
}
