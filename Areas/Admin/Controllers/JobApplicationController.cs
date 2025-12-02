using DACN.DTOs;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Models;
using DACN.Repositories;
using DACN.Service.Email;
using DACN.Service.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using static DACN.Enums.StatusEnums;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class JobApplicationController : Controller
    {
        private readonly HrConfirmEmail emailService;
        private readonly JobApplicationRepository jobApplicationRepository;
        private readonly DepartmentRepository departmentRepository;
        private readonly JobPostingRepository jobPostingRepository;
        private readonly EmployeeRepository employeeRepository;
        private readonly NotificationRepository notificationRepository;
        private readonly UserAccountRepository userAccountRepository;
        private readonly IHubContext<Notifications> hubContext;
        public JobApplicationController(JobApplicationRepository jobApplicationRepository, DepartmentRepository departmentRepository, JobPostingRepository jobPostingRepository,EmployeeRepository employeeRepository, HrConfirmEmail hrConfirmEmail, IHubContext<Notifications> hubContext,NotificationRepository notificationRepository,UserAccountRepository userAccountRepository)
        {
            this.jobApplicationRepository = jobApplicationRepository;
            this.departmentRepository = departmentRepository;
            this.jobPostingRepository = jobPostingRepository;
            this.employeeRepository = employeeRepository;
            this.emailService = hrConfirmEmail;
            this.hubContext = hubContext;
            this.notificationRepository = notificationRepository;
            this.userAccountRepository = userAccountRepository;
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
            return View();
        }
        public async Task<PartialViewResult> ListData(
         int page = 1, int pageSize = 5, string keySearch = "", int status = -1, int departmentId = 0,
         DateTime? PostedDate = null, DateTime? DateTo = null)
        {
            var (data, total) = await jobApplicationRepository.GetPagedAsync(page, pageSize, keySearch, PostedDate, DateTo, status);
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });

            var job = await jobApplicationRepository.GetByIdAsync(id);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            var edu = job.UserAccount?.EducationExperiences?.FirstOrDefault();
            // Map sang DTO

            var jobDto = new JobApplicationRespone
            {
                Id = job.Id,
                AppliedType = job.AppliedType,
                Status = job.Status,
                SubmittedDate = DateTime.Now,
                ReviewedDate = job.ReviewedDate,
                EmployeeId = job.EmployeeId,
                CandidateName = job.UserAccount.FullName??"N/A",
                Skills = job.Skills,
                Notes = job.Notes,
                CvUrl = job.CvFilePath,
                EducationExperience = new EducationExperienceRespone
                {
                    Major = edu?.Major ?? string.Empty,
                    EducationLevel = (EducationLevelEnum)(edu?.EducationLevel), // Giả sử EducationLevel là nullable
                    University = edu?.University ?? string.Empty,
                    GPA = edu?.GPA,
                    GraduationYear = edu?.GraduationYear,
                    ExperienceDescription = edu?.ExperienceDescription ?? string.Empty
                }
            };
            return PartialView("Detail", jobDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });

            var job = await jobApplicationRepository.GetByIdAsync(id);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            var edu = job.UserAccount?.EducationExperiences?.FirstOrDefault();
            // Map sang DTO
            var jobDto = new JobApplicationRespone
            {
                Id = job.Id,
                AppliedType = job.AppliedType,
                Status = job.Status,
                SubmittedDate = job.SubmittedDate,
                ReviewedDate = job.ReviewedDate,
                EmployeeId = job.EmployeeId,
                CandidateName = job.UserAccount.FullName,
                Skills = job.Skills,
                Notes = job.Notes,
                CvUrl = job.CvFilePath,
                EducationExperience = new EducationExperienceRespone
                {
                    EducationExperienceId = edu?.Id ?? 0,
                    Major = edu?.Major ?? string.Empty,
                    EducationLevel = (EducationLevelEnum)(edu?.EducationLevel), // Giả sử EducationLevel là nullable
                    University = edu?.University ?? string.Empty,
                    GPA = edu?.GPA,
                    GraduationYear = edu?.GraduationYear,
                    ExperienceDescription = edu?.ExperienceDescription ?? string.Empty
                }
            };
            return PartialView("Edit", jobDto);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var jobs = jobPostingRepository.QueryAll()
            .Select(j => new JobPostingRespone
            {
                Id = j.Id,
                Title = j.Title
            })
            .ToList();
            var model = new JobApplicationRespone
            {
                JobPostings = jobs
            };
            return PartialView("Create", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int Id,JobApplicationRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            var existing = await jobApplicationRepository.GetByIdAsync(Id);
            if (existing == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ ứng tuyển" });
            try
            {
                await jobApplicationRepository.UpdateAsync(Id,dto);
                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobApplicationRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            //var employee = await employeeRepository.GetByEmailAsync(model.Email);
            //if (employee != null)
            //{
            //    return Json(new { success = false, message = "Email này đã tồn tại trong danh sách nhân viên" });
            //}
            var edu = new EducationExperienceModel
            {
                EducationLevel = model.EducationExperience.EducationLevel,
                Major = model.EducationExperience.Major,
                University = model.EducationExperience.University,
                GraduationYear = model.EducationExperience.GraduationYear,
                GPA = model.EducationExperience.GPA,
                ExperienceDescription = model.EducationExperience.ExperienceDescription
            };

            var jobApp = new JobApplicationModel
            {
                //Email = model.Email,
                EmployeeId = null,
                JobPostingId = model.JobPostingId,
                AppliedType = model.AppliedType,
                Skills = model.Skills,
                CvFilePath = await SaveCvAsync(model.CvFilePath),
                Notes = model.Notes
            };
            try
            {
                await jobApplicationRepository.AddWithEducationAsync(jobApp, edu);

                return Json(new
                {
                    success = true,
                    //message = $"Ứng viên {model.Email} đã ứng tuyển thành công!"
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Lỗi xảy ra trong quá trình thao tác DB (EF Core)
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;

                // Ghi log ra console (hoặc log file)
                Console.WriteLine("🔴 Database error: " + inner);

                return Json(new
                {
                    success = false,
                    message = "Lỗi cơ sở dữ liệu: " + inner
                });
            }
            catch (SqlException sqlEx)
            {
                // Lỗi trực tiếp từ SQL Server
                Console.WriteLine("❌ SQL error: " + sqlEx.Message);

                return Json(new
                {
                    success = false,
                    message = "Lỗi SQL: " + sqlEx.Message
                });
            }
            catch (Exception ex)
            {
                // Các lỗi khác (null, logic, etc.)
                Console.WriteLine("⚠️ General error: " + ex.Message);

                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi không mong muốn: " + ex.Message
                });
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
        public async Task<IActionResult> Status(int? jobId, int? status)
        {
            try
            {
                if (jobId == null || status == null)
                    return Json(new { success = false, message = "ID hoặc trạng thái không hợp lệ" });

                if (!Enum.IsDefined(typeof(ApplicationStatus), status.Value))
                    return Json(new { success = false, message = "Trạng thái không hợp lệ" });

                var jobApp = await jobApplicationRepository.GetFullByIdAsync(jobId.Value);
                if (jobApp == null)
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ ứng tuyển" });

                if (jobApp.UserAccount == null || jobApp.JobPosting == null)
                    return Json(new { success = false, message = "Thiếu thông tin nhân viên hoặc JobPosting" });

                await jobApplicationRepository.UpdateStatusAsync(jobId.Value, (ApplicationStatus)status.Value);
                var employee = await employeeRepository.GetByUserAccount(jobApp.UserAccountId);

                if (status.Value == (int)ApplicationStatus.Approved)
                {
                    // Nếu chưa có Employee → tạo
                    if (employee == null)
                    {
                        var newEmployee = new EmployeeRequest
                        {
                            UserAccountId = jobApp.UserAccountId,
                            Gender = true,
                            DepartmentId = jobApp.JobPosting.DepartmentId,
                            PositionId = jobApp.JobPosting.PositionId,
                            StartDate = DateTime.Now,
                            Address = "Chưa cập nhật",
                            Phone = "Chưa cập nhật",
                            CCCD = "Chưa cập nhật",
                            avatarUrl = "/images/default-avatar.png"
                        };
                        employee = await employeeRepository.CreateEmployeeAsync(newEmployee);
                        // Cập nhật Role UserAccount
                        await userAccountRepository.UpdateUserRoleAsync(jobApp.UserAccountId, UserRole.Employee,employee.EmployeeId);
                    }

                    // Gán EmployeeId và ReviewedDate cho JobApplication (cả 2 trường hợp)
                    jobApp.EmployeeId = employee.EmployeeId;
                    jobApp.ReviewedDate = DateTime.Now;
                    await jobApplicationRepository.UpdateEmployeeAndReviewedDateAsync(jobApp.Id, employee.EmployeeId);
                }
                await emailService.SendApplicationStatusEmailAsync(
                    jobApp.UserAccount.Email,
                    jobApp.UserAccount.FullName,
                    jobApp.JobPosting.Title,
                    (ApplicationStatus)status);

                var notif = new SendNotificationRequest
                {
                    Title = "Cập nhật trạng thái ứng tuyển",
                    Content = $"Hồ sơ ứng tuyển của bạn cho vị trí {jobApp.JobPosting.Title} đã được cập nhật trạng thái.",
                    CreatedAt = DateTime.Now,
                    Type = NotificationType.TuyenDungThanhCong,
                    IsRead = false,
                    EmployeeId = employee?.EmployeeId, 
                    UserAccountId = jobApp.UserAccountId,
                    Url = $"/Employee/Detail/{jobApp.Id}"
                };
                await notificationRepository.CreateNotificationAsync(notif);
                if (hubContext != null)
                {
                    await hubContext.Clients.User(jobApp.UserAccountId.ToString())
                       .SendAsync("NotificationEmployee", notif);
                }

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                // log ex nếu cần
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }


    }
}
