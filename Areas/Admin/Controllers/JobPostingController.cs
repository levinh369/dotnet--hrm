using DACN.Data;
using DACN.DTOs;
using DACN.DTOs.Respone;
using DACN.DTOs.Request;
using DACN.Enums;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using static DACN.Enums.StatusEnums;
namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class JobPostingController : Controller
    {
        private readonly DepartmentRepository departmentRepository;
        private readonly JobPostingRepository jobPostingRepository;
        private readonly PositionRepository positionRepository;
        public JobPostingController(DepartmentRepository departmentRepository, JobPostingRepository jobPostingRepository, PositionRepository positionRepository)
        {
            this.departmentRepository = departmentRepository;
            this.jobPostingRepository = jobPostingRepository;
            this.positionRepository = positionRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý tin tuyển dụng";
            var departmentsList = await departmentRepository.GetAllAsync();
            var departments = departmentsList
                 .Select(d => new SelectOptionDto
                 {
                     Value = d.DepartmentId,
                     Text = d.DepartmentName
                 })
                 .ToList();

            // ⚙️ Lấy danh sách trạng thái từ enum
            var statusList = Enum.GetValues(typeof(JobPostingStatus))
                .Cast<JobPostingStatus>()
                .Select(s => new SelectOptionDto
                {
                    Value = (int)s,
                    Text = s.ToString().Replace("_", " ")
                })
                .ToList();
            // 📦 Gửi qua ViewBag (hoặc có thể tạo ViewModel)
            ViewBag.Departments = departments;
            ViewBag.StatusList = statusList;
            return View();
        }
        public async Task<PartialViewResult> ListData(
         int page = 1, int pageSize = 5, string keySearch = "",int status=-1,int departmentId=0,
         DateTime? PostedDate = null, DateTime? DateTo = null, DateTime? ExpirationDate=null)
        {
            var (entities, total) = await jobPostingRepository.GetPagedAsync(page, pageSize, keySearch, PostedDate, DateTo, status, departmentId,ExpirationDate);
            var data = entities.Select(j => new JobPostingRespone
            {
                Id = j.Id,
                Title = j.Title,
                JobDescription = j.JobDescription,
                PostedDate = j.PostedDate,
                SalaryRange = j.SalaryRange,
                Status = j.Status,
                DepartmentName = j.Department.DepartmentName,
                IsActive = j.IsActive,
                CreateName = j.CreatedBy.Account.FullName,
                positionName = j.Position.PositionName,
                ExpirationDate = j.ExpirationDate,
                // map thêm các trường cần thiết
            }).ToList();
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departmentsList = await departmentRepository.GetAllAsync();
            var departments = departmentsList
                 .Select(d => new SelectOptionDto
                 {
                     Value = d.DepartmentId,
                     Text = d.DepartmentName
                 })
                 .ToList();
            var positionList = await positionRepository.GetAllAsync();
            var positions = positionList
                 .Select(p => new SelectOptionDto
                 {
                     Value = p.PositionId,
                     Text = p.PositionName
                 })
                 .ToList();
            ViewBag.Departments = departments;
            ViewBag.Positions = positions;
            return PartialView("Create");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostingRequest dto)
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

            // 2. Lấy UserId từ Claims
            var employeeIdString = User.FindFirstValue("EmployeeId");
            if (!int.TryParse(employeeIdString, out int employeeId))
            {
                return Json(new { success = false, message = "Không xác định được EmployeeId" });
            }
            dto.CreateById = employeeId;

            // 3. Gọi Repository
            try
            {
                await jobPostingRepository.CreateJobPostingAsync(dto);

                return Json(new { success = true, message = "Đã tạo CV thành công!" });
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

            var job = await jobPostingRepository.GetByIdAsync(id);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });

            // Map sang DTO
            var jobDto = new JobPostingRespone
            {
                Id = job.Id,
                Title = job.Title,
                SalaryRange = job.SalaryRange,
                JobDescription = job.JobDescription,
                Requirements = job.Requirements,
                PostedDate = job.PostedDate,
                ExpirationDate = job.ExpirationDate,
                UpdateAt = job.UpdateAt,
                Status = job.Status,
                DepartmentName = job.Department?.DepartmentName,
                positionName = job.Position?.PositionName,
                CreateName = job.CreatedBy?.Account.FullName, // nếu có trường FullName
                ViewCount = job.ViewCount ?? 0,
                IsActive = job.IsActive,
                IsDeleted = job.IsDeleted,
                EmploymentType = job.EmploymentType,
                ExperienceLevel = job.RequiredExperience
            };

            return PartialView("Detail", jobDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });
            var job = await jobPostingRepository.GetByIdAsync(id);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            try
            {
                var departmentsList = await departmentRepository.GetAllAsync();
                var departments = departmentsList
                     .Select(d => new SelectOptionDto
                     {
                         Value = d.DepartmentId,
                         Text = d.DepartmentName
                     })
                     .ToList();

                // ⚙️ Lấy danh sách trạng thái từ enum
                var statusList = Enum.GetValues(typeof(JobPostingStatus))
                    .Cast<JobPostingStatus>()
                    .Select(s => new SelectOptionDto
                    {
                        Value = (int)s,
                        Text = s switch
                        {
                            JobPostingStatus.DangMo => "Đang mở",
                            JobPostingStatus.Dong => "Đóng",
                            JobPostingStatus.HetHan => "Hết hạn",
                            _ => "Không xác định"
                        }
                    })
                    .ToList();
                var positionList = await positionRepository.GetAllAsync();
                var positions = positionList
                     .Select(p => new SelectOptionDto
                     {
                         Value = p.PositionId,
                         Text = p.PositionName
                     })
                     .ToList();
                // 📦 Gửi qua ViewBag (hoặc có thể tạo ViewModel)
                ViewBag.Departments = departments;
                ViewBag.StatusList = statusList;
                ViewBag.Positions = positions;
                var jobDto = new JobPostingRespone
                {
                    Id = job.Id,
                    Title = job.Title,
                    SalaryRange = job.SalaryRange,
                    JobDescription = job.JobDescription,
                    Requirements = job.Requirements,
                    PostedDate = job.PostedDate,
                    ExpirationDate = job.ExpirationDate,
                    Status = job.Status,
                    DepartmentName = job.Department?.DepartmentName,
                    positionName = job.Position?.PositionName,
                    ViewCount = job.ViewCount ?? 0,
                    IsActive = job.IsActive,
                    DepartmentId = job.Department.DepartmentId,
                    PositionId = job.Position.PositionId,
                    EmploymentType = job.EmploymentType,
                    ExperienceLevel = job.RequiredExperience
                };

                return PartialView("Edit", jobDto);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(JobPostingRequest dto)
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
                await jobPostingRepository.UpdateJobPostingAsync(dto);
                return Json(new { success = true, message = "Cập nhật tin tuyển dụng thành công!" });
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
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });
            try
            {
                await jobPostingRepository.DeleteJobPostingAsync(id);
                return Json(new { success = true, message = "Xóa tin tuyển dụng thành công!" });
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
