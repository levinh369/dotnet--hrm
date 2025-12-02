using DACN.DTOs.Request;
using DACN.Repositories;
using DACN.Service.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DACN.Areas.Employee.Controllers
{
    public class AuthController : Controller
    {
        private readonly EmployeeRepository employeeRepository;
        public AuthController(EmployeeRepository employeeRepository)
        {  
            this.employeeRepository = employeeRepository;
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
            TempData["Success"] = "Kích hoạt tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Account", "Login"); // Giả sử bạn có Action Login
        }
    }
}
