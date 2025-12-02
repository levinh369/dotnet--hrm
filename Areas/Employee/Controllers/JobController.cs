using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Models;
using DACN.Repositories;
using DACN.Service.Email;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACN.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class JobController : Controller
    {
        private readonly JobPostingRepository jobPostingRepository;
        private readonly JobApplicationRepository jobApplicationRepository;
        private readonly ApplicationConfirmationEmail emailService;
        private readonly EmployeeRepository employeeRepository;
        public JobController(JobPostingRepository jobPostingRepository, JobApplicationRepository jobApplicationRepository, ApplicationConfirmationEmail applicationConfirmationEmail, EmployeeRepository employeeRepository)
        {
            this.jobPostingRepository = jobPostingRepository;
            this.jobApplicationRepository = jobApplicationRepository;
            this.emailService = applicationConfirmationEmail;
            this.employeeRepository = employeeRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int jobId)
        {
            if(jobId <= 0)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            }
            try
            {
                var job = await jobPostingRepository.GetByIdAsync(jobId);
                if (job == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy công việc" });
                }
                var response = new JobPostingRespone
                {
                    Id = job.Id,
                    Title = job.Title,
                    JobDescription = job.JobDescription,
                    SalaryRange = job.SalaryRange,
                    CreateName = job.CreatedBy?.Account?.FullName,
                    DepartmentName = job.Department?.DepartmentName,
                    positionName = job.Position?.PositionName,
                    PostedDate = job.PostedDate,
                    ExpirationDate = job.ExpirationDate,
                    ViewCount = job.ViewCount ?? 0,
                    Requirements = job.Requirements,
                    // map thêm các field khác nếu có
                };


                return View("Detail", response);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải dữ liệu", error = ex.Message });
            }
        }
        public async Task<IActionResult> Index(
         int page = 1, int pageSize = 5, string query = "")
        {
            var (entities, total) = await jobPostingRepository.SearchJobs(page, pageSize, query);
            var data = entities.Select(j => new JobPostingRespone
            {
                Id = j.Id,
                Title = j.Title,
                PostedDate = j.PostedDate,
                SalaryRange = j.SalaryRange,
                Status = j.Status,
                DepartmentName = j.Department.DepartmentName,
                positionName = j.Position.PositionName,
                ExpirationDate = j.ExpirationDate,
                // map thêm các trường cần thiết
            }).ToList();
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return View(data);
        }
        public async Task<IActionResult> ApplyJob(int jobId)
        {
            if (jobId <= 0)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            }
            try
            {
                var job = await jobPostingRepository.GetJobByIdAsync(jobId);
                if (job == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi" });
                }
                var jobResponse = new JobPostingRespone
                {
                    Id = job.Id,
                    Title = job.Title,
                    ExperienceLevel = job.RequiredExperience,
                    EmploymentType = job.EmploymentType,
                };
                return PartialView("Apply", jobResponse);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi tải dữ liệu",
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(JobApplicationRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    foreach (var error in errors)
                    {
                        Console.WriteLine("ModelState error: " + error);
                    }

                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors });
                }

                var userAccountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userAccountIdString, out int userAccountId))
                {
                    return Json(new { success = false, message = "Không xác định được UserAccountId" });
                }

                var employee = await employeeRepository.GetByUserAccount(userAccountId);
                if (employee != null)
                {
                    model.EmployeeId = employee.EmployeeId;
                }

                var edu = new EducationExperienceModel
                {
                    UserAccountId = userAccountId,
                    EducationLevel = model.EducationExperience.EducationLevel,
                    Major = model.EducationExperience.Major,
                    University = model.EducationExperience.University,
                    GraduationYear = model.EducationExperience.GraduationYear,
                    GPA = model.EducationExperience.GPA,
                    ExperienceDescription = model.EducationExperience.ExperienceDescription
                };

                var jobApp = new JobApplicationModel
                {
                    UserAccountId = userAccountId,
                    EmployeeId = employee?.EmployeeId,
                    JobPostingId = model.JobPostingId,
                    AppliedType = model.AppliedType,
                    Skills = model.Skills,
                    CvFilePath = await SaveCvAsync(model.CvFilePath),
                    Notes = model.Notes
                };

                await jobApplicationRepository.AddWithEducationAsync(jobApp, edu);

                var email = User.FindFirstValue(ClaimTypes.Email);
                var fullName = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                {
                    return Json(new { success = false, message = "Không xác định được Email hoặc FullName" });
                }

                var jobPosting = await jobPostingRepository.GetByIdAsync(model.JobPostingId);
                if (jobPosting == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy Job Posting" });
                }

                await emailService.SendOrderConfirmationAsync(email, fullName, jobPosting.Title);

                return Json(new { success = true, message = "Ứng tuyển thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                // Lỗi liên quan đến EF, ví dụ foreign key, null, duplicate
                Console.WriteLine("EF error: " + dbEx.InnerException?.Message ?? dbEx.Message);
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
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

    }
}
