using DACN.DTOs;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AttendanceController : Controller
    {
        private readonly AttendanceRepository attendanceRepository;
        private readonly DepartmentRepository departmentRepository;
        public AttendanceController(AttendanceRepository attendanceRepository, DepartmentRepository departmentRepository)
        {
            this.attendanceRepository = attendanceRepository;
            this.departmentRepository = departmentRepository;
        }
        public async Task<IActionResult> Index()
        {
            var departmentsList = await departmentRepository.GetAllAsync();
            var departments = departmentsList
                 .Select(d => new SelectOptionDto
                 {
                     Value = d.DepartmentId,
                     Text = d.DepartmentName
                 })
                 .ToList();
            ViewBag.Departments = departments;
            var stats = await attendanceRepository.GetAttendanceSummaryAsync(DateTime.Now);
            stats.Date = DateTime.Now;
            return View(stats);
        }
        public async Task<PartialViewResult> ListData(
        int page = 1, int pageSize = 5, string keySearch = "",
        DateTime? fromDate = null, int? departmentId = null)
        {
            var (entities, total) = await attendanceRepository.GetPagedAsync(page, pageSize, keySearch, fromDate, departmentId);
            var data = entities.Select(j => new AttendanceDetailRespone
            {
                Id = j.Id,
                EmployeeId = j.EmployeeId,
                EmployeeName = j.Employee.Account.FullName,
                DepartmentName = j.Employee.Department.DepartmentName,
                CheckIn = j.RawCheckInTime ?? j.CheckInTime,
                CheckOut = j.RawCheckOutTime ?? j.CheckOutTime,
                Status = j.Status,
                Note = j.Notes,
                WorkingHours = (double)j.WorkingHours,
            }).ToList();
            if (fromDate != null)
            {
                var stats = await attendanceRepository.GetAttendanceSummaryAsync(fromDate);
                stats.Date = (DateTime)fromDate;
                ViewBag.Stats = stats;
            }
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "ID không hợp lệ" });

            var attendance = await attendanceRepository.GetByIdAsync(id);
            if (attendance == null) return Json(new { success = false, message = "Không tìm thấy" });

            TimeSpan startShift = new TimeSpan(8, 0, 0);
            TimeSpan endShift = new TimeSpan(17, 0, 0);
            TimeSpan? finalCheckIn = attendance.RawCheckInTime ?? attendance.CheckInTime;
            TimeSpan? finalCheckOut = attendance.RawCheckOutTime ?? attendance.CheckOutTime;
            int lateMinutes = 0;
            bool isLate = false;
            int earlyMinutes = 0;
            bool isLeftEarly = false;

            if (finalCheckIn.HasValue && finalCheckIn.Value > startShift)
            {
                lateMinutes = (int)(finalCheckIn.Value - startShift).TotalMinutes;
                isLate = true;
            }

            if (finalCheckOut.HasValue && finalCheckOut.Value < endShift)
            {
                earlyMinutes = (int)(endShift - finalCheckOut.Value).TotalMinutes;
                isLeftEarly = true;
            }

            var data = new AttendanceDetailRespone
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                EmployeeName = attendance.Employee.Account.FullName,
                Avatar = attendance.Employee.AvatarUrl,
                DepartmentName = attendance.Employee.Department.DepartmentName,
                PositionName = attendance.Employee.Position.PositionName,
                WorkDate = attendance.WorkDate,
                CheckIn = finalCheckIn,
                CheckOut = finalCheckOut,

                RawCheckIn = attendance.CheckInTime,
                RawCheckOut = attendance.CheckOutTime,
                EditReason = attendance.EditReason,
                EditedBy = attendance.EditedBy,
                WorkingHours = attendance.WorkingHours ?? 0,
                Status = attendance.Status,
                Note = attendance.Notes,
                IsLate = isLate,
                LateMinutes = lateMinutes,
                IsLeftEarly = isLeftEarly,
                EarlyMinutes = earlyMinutes,
                IsCheckInEdited = attendance.RawCheckInTime.HasValue,
                IsCheckOutEdited = attendance.RawCheckOutTime.HasValue,
            };

            return PartialView("Detail", data);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "ID không hợp lệ" });
            var attendance = await attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi chấm công" });
            var attendanceDto = new AttendanceDetailRespone
            {
                Id = attendance.Id,
                EmployeeName = attendance.Employee.Account.FullName,
                WorkDate = attendance.WorkDate,
                CheckIn = attendance.RawCheckInTime??attendance.CheckInTime,
                CheckOut = attendance.RawCheckOutTime??attendance.CheckOutTime,
                WorkingHours = (double)attendance.WorkingHours,
                EditReason = attendance.EditReason,
            };
            return PartialView("Edit", attendanceDto);
        }
        [HttpPost]
        public async Task<IActionResult> EditPost(int id, AttendaceRequest dto)
        {
            if (id <= 0)
                return Json(new { success = false, message = "ID không hợp lệ" });
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            var adminName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(adminName))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng F5" });
            }
            dto.EditedBy = adminName.Trim();
            try
                {
                    await attendanceRepository.UpdateAttendancetAsync(id, dto);
                    return Json(new { success = true, message = "Cập nhật chấm công thành công" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
        }
    }
}
