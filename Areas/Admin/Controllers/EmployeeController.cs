using DACN.DTOs;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Repositories;
using DACN.Service.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.Intrinsics.Arm;
using static DACN.Enums.StatusEnums;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly DepartmentRepository departmentRepository;
        private readonly EmployeeRepository employeeRepository;
        private readonly PositionRepository positionRepository;
        private readonly SendWelcomeEmail sendWelcomeEmail;
        public EmployeeController(DepartmentRepository departmentRepository, EmployeeRepository employeeRepository, PositionRepository positionRepository,SendWelcomeEmail sendWelcomeEmail)
        {
            this.departmentRepository = departmentRepository;
            this.employeeRepository = employeeRepository;
            this.positionRepository = positionRepository;
            this.sendWelcomeEmail = sendWelcomeEmail;
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
            var positionsList = await positionRepository.GetAllAsync();
            var positions = positionsList
                 .Select(d => new SelectOptionDto
                 {
                     Value = d.PositionId,
                     Text = d.PositionName
                 })
                 .ToList();
            // 📦 Gửi qua ViewBag (hoặc có thể tạo ViewModel)
            ViewBag.Departments = departments;
            ViewBag.Positions = positions;
            return View();
        }
        public async Task<PartialViewResult> ListData(
         int page = 1, int pageSize = 5, string keySearch = "", int IsActive = -1, int departmentId = 0, int positionId = 0,
         string? cccd = null, string? email = null, DateTime? fromDate = null, DateTime? DateTo = null)
        {
            var (data, total) = await employeeRepository.GetPagedAsync(page, pageSize, keySearch, IsActive, departmentId, positionId, cccd, email, fromDate, DateTo);
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        public async Task<IActionResult> Create()
        {
            var departmentsList = await departmentRepository.GetAllAsync();
            ViewBag.Departments = departmentsList
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                })
                .ToList();

            var positionsList = await positionRepository.GetAllAsync();
            ViewBag.Positions = positionsList
                .Select(p => new SelectListItem
                {
                    Value = p.PositionId.ToString(),
                    Text = p.PositionName
                })
                .ToList();
            return PartialView("create");
        }
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "ID không hợp lệ" });

            var emp = await employeeRepository.GetByIdAsync(id);
            if (emp == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });

            var applications = emp.Account.JobApplications
                .OrderByDescending(j => j.SubmittedDate)
                .Select(j => new JobApplicationDto
                {
                    JobTitle = j.JobPosting?.Title,
                    SubmittedDate = j.SubmittedDate,
                    CvFilePath = j.CvFilePath,
                    Skills = j.Skills,
                    Notes = j.Notes,
                    RecruiterName = j.JobPosting?.CreatedBy?.Account?.FullName,
                    Status = j.Status switch
                    {
                        ApplicationStatus.Pending => "Chờ duyệt",    // Hoặc "Đang xem xét"
                        ApplicationStatus.Approved => "Trúng tuyển", // Hoặc "Đã duyệt"
                        ApplicationStatus.Rejected => "Từ chối",     // Hoặc "Không đạt"
                        _ => "Khác"
                    },
                })
                .ToList();
            var contracts = emp.Contracts
                .Where(c=>!c.IsDeleted)
                .OrderByDescending(j => j.SignedDate)
                .Select(j => new ContractDTO
                {
                    ContractCode = j.ContractCode,
                    StartDate = j.StartDate,
                    EndDate = j.EndDate,
                    BaseSalary = j.BasicSalary,
                    Status = j.Status,
                    Type = j.Type switch
                    {
                        ContractType.ThuViec => "Thử việc",
                        ContractType.CoThoiHan => "Có thời hạn",
                        ContractType.ChinhThuc => "Chính thức",
                        ContractType.ThoiVu => "Thời vụ",
                        _ => "Khác"
                    },
                    FilePath = j.FilePath,
                    SignedDate = j.SignedDate
                })
                .ToList();
            var eduExp = emp.Account?.EducationExperiences?.FirstOrDefault();
            var empDto = new EmployeeDetailRespone
            {
                Email = emp.Account?.Email,
                DepartmentName = emp.Department?.DepartmentName,
                PositionName = emp.Position?.PositionName,
                FullName = emp.Account?.FullName,
                Dob = emp.DateOfBirth,
                Address = emp.Address,
                Phone = emp.Phone,
                CCCD = emp.CCCD,
                StartDate = emp.StartDate,
                Avatar = emp.AvatarUrl,
                Gender = emp.Gender,
                status = emp.Status,
                EducationLevel = eduExp.EducationLevel,
                Major = eduExp?.Major,
                University = eduExp?.University,
                GPA = eduExp?.GPA,
                GraduationYear = eduExp?.GraduationYear,
                ExperienceDescription = eduExp?.ExperienceDescription,
                Contracts = contracts,
                jobApplications = applications
            };

            return PartialView("Detail", empDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });

            var emp = await employeeRepository.GetByIdAsync(id);
            if (emp == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });

            var selectedApplication = emp.Account?.JobApplications?
            .FirstOrDefault(j => j.Status == ApplicationStatus.Approved)
            ?? emp.Account?.JobApplications?.FirstOrDefault();
            //var allEducationList = emp.Account?.EducationExperiences;
            var eduExp = emp.Account?.EducationExperiences?.FirstOrDefault();

            // --- Lấy danh sách phòng ban và chức vụ ---
            var departmentsList = await departmentRepository.GetAllAsync();
            var positionsList = await positionRepository.GetAllAsync();

            // Sử dụng SelectListItem để Razor hiểu được
            ViewBag.Departments = departmentsList
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();

            ViewBag.Positions = positionsList
                .Select(p => new SelectListItem
                {
                    Value = p.PositionId.ToString(),
                    Text = p.PositionName
                }).ToList();

            // --- Map sang DTO ---
            var empDto = new EmployeeDetailRespone
            {
                EducationExperienceId = eduExp.Id,
                EmployeeId = emp.EmployeeId,
                UserAccountId = emp.UserAccountId,
                DepartmentId = emp.Department?.DepartmentId ?? 0,
                PositionId = emp.Position?.PositionId ?? 0,
                Email = emp.Account?.Email,
                DepartmentName = emp.Department?.DepartmentName,
                PositionName = emp.Position?.PositionName,
                FullName = emp.Account?.FullName,
                Dob = emp.DateOfBirth,
                Address = emp.Address,
                Phone = emp.Phone,
                CCCD = emp.CCCD,
                StartDate = emp.StartDate,
                Avatar = emp.AvatarUrl,
                Gender = emp.Gender,
                status = emp.Status,
                AppliedJobTitle = selectedApplication?.JobPosting?.Title,
                AppliedDate = selectedApplication?.SubmittedDate,
                CvFilePath = selectedApplication?.CvFilePath,
                Skills = selectedApplication?.Skills,
                Note = selectedApplication?.Notes,
                EducationLevel = eduExp.EducationLevel,
                Major = eduExp?.Major,
                University = eduExp?.University,
                GPA = eduExp?.GPA,
                GraduationYear = eduExp?.GraduationYear,
                ExperienceDescription = eduExp?.ExperienceDescription,
            };

            return PartialView("Edit", empDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(EmployeeRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            var existing = await employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (existing == null) return Json(new { success = false, message = "Nhân viên không tồn tại" });
            try
            {
                await employeeRepository.UpdateEmployeeAndExperienceAsync(dto);
                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeRequest dto)
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
                var emp = await employeeRepository.addEmployee(dto);
                string activationLink = Url.Action(
                action: "ActivateAccount",
                controller: "Account",
                // SỬA Ở ĐÂY: Dùng ?. để tránh lỗi nếu Account null, và ?? "" để tránh lỗi nếu Token null
                values: new { token = emp.ActivationToken ?? "" },
                protocol: Request.Scheme
                ) ?? "";
                var positionObj = await positionRepository.GetByIdAsync(dto.PositionId ?? 0);
                string posName = positionObj?.PositionName ?? "Nhân viên mới"; // Lấy thuộc tính .PositionName
                var departmentObj = await departmentRepository.GetByIdAsync(dto.DepartmentId ?? 0);
                string deptName = departmentObj?.DepartmentName ?? "Chưa cập nhật"; // Lấy thuộc tính .DepartmentName
                // Gửi email chào mừng
                await sendWelcomeEmail.SendWelcomeEmailAsync(
                    emp.Email,
                    emp.FullName,
                    posName,
                    deptName,
                    dto.StartDate,
                    activationLink 
                );
                return Json(new { success = true, message = "Tạo nhân viên thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo nhân viên: " + ex.Message });
            }
        }
    }
}
