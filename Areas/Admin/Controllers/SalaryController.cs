using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using static DACN.Enums.StatusEnums;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SalaryController : Controller
    {
        private readonly SalaryRepository salaryRepository;
        public SalaryController(SalaryRepository salaryRepository)
        {
            this.salaryRepository = salaryRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<PartialViewResult> ListData(
        int page = 1, int pageSize = 5, int month = 0, int year = 0)
        {
            var (entities, total) = await salaryRepository.GetPagedAsync(page, pageSize, month, year);
            var data = entities.Select(j => new SalaryRespone
            {
                SalaryId = j.SalaryId,
                EmployeeId = j.EmployeeId,
                EmployeeName = j.Employee.Account.FullName,
                DepartmentName = j.Employee.Department.DepartmentName,
                BaseSalary = j.BaseSalary,
                Allowance = j.Allowance,
                Deduction = j.Deduction,
                NetSalary = j.NetSalary,
                WorkDays = (int)j.ActualWorkDays,
                Avatar = j.Employee.AvatarUrl,
                SalaryStatus = j.Status,
            }).ToList();
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        [HttpPost]
        public async Task<IActionResult> CalculatePayroll(int month, int year)
        {
            if (month < 1 || month > 12 || year < 2000)
                return Json(new { success = false, message = "Tháng hoặc năm không hợp lệ." });
            try
            {
                var result = await salaryRepository.CalculateAndSavePayrollAsync(month, year);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình tính lương: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Id không hợp lệ" });
            }
            var salaryDetail = await salaryRepository.GetDetailSalary(id);
            if (salaryDetail == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            }
            return PartialView(salaryDetail);
        }
        [HttpPost]
        public async Task<IActionResult> EditPost(int id, SalaryRequest dto)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Id không hợp lệ" });
            }
            try
            {
                var userUpdating = User.Identity?.Name ?? "Unknown";
                await salaryRepository.UpdateSalaryById(id, dto, userUpdating);
                return Json(new { success = true, message = "Cập nhật bảng lương thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật bảng lương: " + ex.Message });
            }
        }
    }
}
