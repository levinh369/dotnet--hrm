using DACN.DTOs.Request;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DACN.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class EmployeeController : Controller
    {
        private readonly AttendanceRepository attendanceRepository;
        private readonly JobApplicationRepository applicationRepository;
        public EmployeeController(AttendanceRepository attendanceRepository, JobApplicationRepository applicationRepository)
        {
            this.attendanceRepository = attendanceRepository;
            this.applicationRepository = applicationRepository;
        }
        public async Task<IActionResult> Attendance()
        {
            var userId = int.Parse(User.FindFirstValue("EmployeeId"));

            // Gọi hàm bạn vừa viết
            var attendance = await attendanceRepository.GetTodayAttendanceAsync(userId);

            if (attendance == null)
            {
  
                ViewBag.HasCheckIn = false;
                ViewBag.HasCheckOut = false;
            }
            else if (attendance.CheckOutTime == null)
            {

                ViewBag.HasCheckIn = true;   // Ẩn nút Check-in
                ViewBag.HasCheckOut = false; // Hiện nút Check-out (Logic View của bạn là false = hiện nút Check-out đúng ko?)

            }
            else
            {
                // TH3: Đã xong việc (Đã check-out rồi)
                ViewBag.HasCheckIn = true;
                ViewBag.HasCheckOut = true;
            }
            return View();

        }
        public IActionResult Salary()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CheckIn(AttendaceRequest dto)
        {
            var employeeIdString = User.FindFirstValue("EmployeeId");
            if (!int.TryParse(employeeIdString, out int employeeId))
            {
                return Json(new { success = false, message = "Lỗi xác thực: Không tìm thấy thông tin nhân viên." });
            }
            dto.EmployeeId = employeeId;
            try
            {
                await attendanceRepository.CheckInAsync(dto);
                return Json(new
                {
                    success = true,
                    message = "Check-in thành công!",
                    time = DateTime.Now.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckOut(AttendaceRequest dto)
        {
            var employeeIdString = User.FindFirstValue("EmployeeId");
            if (!int.TryParse(employeeIdString, out int employeeId))
            {
                return Json(new { success = false, message = "Lỗi xác thực: Không tìm thấy thông tin nhân viên." });
            }
            dto.EmployeeId = employeeId;
            try
            {
                await attendanceRepository.CheckOutAsync(dto);
                return Json(new
                {
                    success = true,
                    message = "Check-out thành công!",
                    time = DateTime.Now.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id=0)
        {
            if(id<=0)
            {
                return Json(new { success = true, message = "ID không tồn tại!" });
            }
            var applicationDetail = await applicationRepository.GetAppByEmployee(id);
            return View(applicationDetail);
        }
    }
}

