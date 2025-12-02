using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Repositories;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using static DACN.Enums.StatusEnums;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContractController : Controller
    {
        private readonly ContractRepository contractRepository;
        private readonly EmployeeRepository employeeRepository;
        public ContractController(ContractRepository contractRepository, EmployeeRepository employeeRepository)
        {
            this.contractRepository = contractRepository;
            this.employeeRepository = employeeRepository;
        }
        public async Task<IActionResult> Index()
        {
            var stats = await contractRepository.GetContractSummaryAsync();
            return View(stats);
        }
        public async Task<IActionResult> CreateAsync()
        {
            var employees = await employeeRepository.GetEmployeesByDepartmentAsync();
            var list = employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                // Giả sử bạn có trường EmployeeCode, nếu không thì dùng ID
                Text = $"NV{e.EmployeeId} - {e.Account.FullName} ({e.Department.DepartmentName})"
            }).ToList();
            string newCode = await contractRepository.GetNextContractCodeAsync();
            // Gán vào ViewBag thay vì Model
            ViewBag.NewContractCode = newCode;
            // 4. Đẩy sang View (Thường dùng ViewBag)
            ViewBag.EmployeeList = list;
            return PartialView();
        }
        public async Task<PartialViewResult> ListData(
         int page = 1, int pageSize = 5, string keySearch = "",
         DateTime? fromDate = null, DateTime? toDate = null, int contractType = -1, int status = -1)
        {
            var today = DateTime.Now.Date;
            var warningDate = today.AddDays(30);
            var (entities, total) = await contractRepository.GetPagedAsync(page, pageSize, keySearch, fromDate, toDate, contractType, status);
            var data = entities.Select(j => new ContractRespone
            {
                ContractId = j.ContractId,
                ContractCode = j.ContractCode,
                EmployeeName = j.Employee.Account.FullName,
                DepartmentName = j.Employee.Department.DepartmentName,
                PositionName = j.Employee.Position.PositionName,
                AvatarUrl = j.Employee.AvatarUrl, // Đừng quên avatar
                ContractType = j.Type,
                SignedDate = j.SignedDate,
                BasicSalary = j.BasicSalary,
                Status = (
                    // Ưu tiên 1: Nếu DB đã lưu là Hết Hạn hoặc Sắp Hết Hạn (hoặc Đã Hủy) -> Giữ nguyên
                    j.Status == ContractStatus.HetHan || j.Status == ContractStatus.ChuaHieuLuc
                    ? j.Status
                    // Ưu tiên 2: Tính toán thực tế - Nếu đã quá ngày kết thúc -> Ép thành Hết Hạn
                    : (j.EndDate.HasValue && j.EndDate.Value < today)
                        ? ContractStatus.HetHan
                    // Ưu tiên 3: Tính toán cảnh báo - Nếu còn hạn nhưng <= 30 ngày -> Ép thành Sắp Hết Hạn
                    : (j.Status == ContractStatus.ConHieuLuc && j.EndDate.HasValue && j.EndDate.Value <= warningDate)
                        ? ContractStatus.ChuaHieuLuc
                    : j.Status
                ), 
                StartDate = j.StartDate,
                EndDate = j.EndDate,
                FilePath = j.FilePath
            }).ToList();
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractRequest dto)
        {
            // 1. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }
            try
            {
                dto.FileUrl = await SaveCvAsync(dto.FileContract);
                await contractRepository.CreateContractAsync(dto);

                return Json(new { success = true, message = "Đã tạo hợp đồng thành công!" });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });

            var con = await contractRepository.GetByIdAsync(id);
            if (con == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });

            // Map sang DTO
            var today = DateTime.Now.Date;
            var warningDate = today.AddDays(30);
            // Map sang DTO
            var conDto = new ContractRespone
            {
                ContractId = con.ContractId,
                ContractCode = con.ContractCode,
                EmployeeName = con.Employee.Account.FullName,
                DepartmentName = con.Employee.Department.DepartmentName,
                PositionName = con.Employee.Position.PositionName,
                AvatarUrl = con.Employee.AvatarUrl, // Đừng quên avatar
                Note = con.Note,
                CreatedAt = con.CreatedAt,
                ContractType = con.Type,
                SignedDate = con.SignedDate,
                BasicSalary = con.BasicSalary,
                Status = (
                    // Ưu tiên 1: Nếu DB đã lưu là Hết Hạn hoặc Sắp Hết Hạn (hoặc Đã Hủy) -> Giữ nguyên
                    con.Status == ContractStatus.HetHan || con.Status == ContractStatus.ChuaHieuLuc
                    ? con.Status
                    // Ưu tiên 2: Tính toán thực tế - Nếu đã quá ngày kết thúc -> Ép thành Hết Hạn
                    : (con.EndDate.HasValue && con.EndDate.Value < today)
                        ? ContractStatus.HetHan
                    // Ưu tiên 3: Tính toán cảnh báo - Nếu còn hạn nhưng <= 30 ngày -> Ép thành Sắp Hết Hạn
                    : (con.Status == ContractStatus.ConHieuLuc && con.EndDate.HasValue && con.EndDate.Value <= warningDate)
                        ? ContractStatus.ChuaHieuLuc
                    : con.Status
                ),
                StartDate = con.StartDate,
                EndDate = con.EndDate,
                FilePath = con.FilePath
            };
            return PartialView("Detail", conDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });
            var con = await contractRepository.GetByIdAsync(id);
            if (con == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            
            // Map sang DTO
            var conDto = new ContractRespone
            {
                ContractId = con.ContractId,
                ContractCode = con.ContractCode,
                EmployeeName = con.Employee.Account.FullName,
                DepartmentName = con.Employee.Department.DepartmentName,
                PositionName = con.Employee.Position.PositionName,
                ContractType = con.Type,
                SignedDate = con.SignedDate,
                Status = con.Status,
                Note = con.Note,
                BasicSalary = con.BasicSalary,
                StartDate = con.StartDate,
                EndDate = con.EndDate,
                FilePath = con.FilePath

            };
            return PartialView("Edit", conDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int ContractId, ContractRequest dto)
        {
            // 1. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            // 2. Gọi Repository
            try
            {
                await contractRepository.UpdateContractAsync(ContractId, dto);
                return Json(new { success = true, message = "Cập nhật thông tin hợp đồng thành công!" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        private async Task<string> SaveCvAsync(IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            return "/uploads/cvs/" + fileName; // path có thể lưu trong DB
        }
        public async Task<IActionResult> renew(int contractId)
        {
            if(contractId <= 0)
                return Json(new { success = false, message = "ID không hợp lệ" });
            var con = await contractRepository.GetByIdAsync(contractId);
            if (con == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            var suggestStartDate = con.EndDate.HasValue ? con.EndDate.Value.AddDays(1) : DateTime.Now;
            // Map sang DTO
            var conDto = new ContractRespone
            {
                EmployeeId = con.EmployeeId,
                ContractId = con.ContractId,
                ContractCode = con.ContractCode,
                ContractCodeNew = await contractRepository.GetNextContractCodeAsync(),
                EmployeeName = con.Employee.Account.FullName,
                DepartmentName = con.Employee.Department.DepartmentName,
                PositionName = con.Employee.Position.PositionName,
                Note = con.Note,
                SignedDate = DateTime.Now, // Ngày ký là hôm nay
                StartDate = suggestStartDate, // Ngày bắt đầu nối tiếp
                EndDate = suggestStartDate.AddMonths(12).AddDays(-1),
                ContractType = con.Type,
                BasicSalary = con.BasicSalary,
                FilePath = con.FilePath,
                Status = con.Status
            };

            return PartialView("renew", conDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewPost(ContractRequest dto)
        {
            // 1. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            // 2. Gọi Repository
            try
            {
                dto.FileUrl = await SaveCvAsync(dto.FileContract);
                await contractRepository.RenewContractTransactionAsync(dto);
                return Json(new { success = true, message = "Gia hạn / thêm hợp đồng thành công!" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        
    }
}
