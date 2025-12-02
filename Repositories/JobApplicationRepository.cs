using DACN.Data;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class JobApplicationRepository
    {
        private readonly ApplicationDbContext db;

        public JobApplicationRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        /// <summary>
        /// Lưu JobApplication và EducationExperience trong 1 transaction
        /// </summary>
        /// 
        public async Task<JobApplicationModel?> GetByIdAsync(int id)
        {
            return await db.JobApplications
        .Include(j => j.Employee)
            .ThenInclude(e => e.Account) // 'e' là Employee, load Account của Employee
        .Include(j => j.UserAccount)
            .ThenInclude(u => u.EducationExperiences) // 'u' là UserAccount, load ds học vấn

        .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
        }
        public async Task<JobApplicationModel?> GetFullByIdAsync(int id)
        {
            return await db.JobApplications
                .Include(j => j.UserAccount) // Tải UserAccount của đơn ứng tuyển
                    .ThenInclude(u => u.EducationExperiences) // TỪ UserAccount, tải danh sách học vấn

                .Include(j => j.JobPosting) // Giữ nguyên

                .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
        }

        public async Task AddWithEducationAsync(JobApplicationModel jobApp, EducationExperienceModel? edu = null)
        {

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (edu != null)
                {
                    db.educationExperiences.Add(edu);
                }
                db.JobApplications.Add(jobApp);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback nếu lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<(List<JobApplicationRespone> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, DateTime? toDate, int status)
        {
            var query = db.JobApplications
            .Include(j => j.UserAccount) // Tải UserAccount của đơn ứng tuyển
                .ThenInclude(u => u.EducationExperiences)
            .Include(j => j.Employee)
                .ThenInclude(e => e.Account)

            .Where(j => !j.IsDeleted)
            .AsQueryable();

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(j => j.Employee.Account.FullName.Contains(keySearch));

            if (fromDate.HasValue)
                query = query.Where(j => j.SubmittedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(j => j.ReviewedDate <= toDate.Value);
            if (status != -1)
                query = query.Where(j => (int)j.Status == status);
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(j => j.SubmittedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .Select(j => new JobApplicationRespone
                 {
                     Id = j.Id,
                     CandidateName = j.UserAccount.FullName ?? "Ứng viên chưa đăng kí",
                     SubmittedDate = j.SubmittedDate,
                     Status = j.Status,
                     CvUrl = j.CvFilePath,
                     EducationExperience = j.UserAccount.EducationExperiences
                            .Select(edu => new EducationExperienceRespone
                            {
                                Major = edu.Major ?? string.Empty,
                                EducationLevel = edu.EducationLevel,
                                University = edu.University ?? string.Empty,
                                GPA = edu.GPA,
                                GraduationYear = edu.GraduationYear,
                                ExperienceDescription = edu.ExperienceDescription ?? string.Empty
                            })
                            .FirstOrDefault() // Lấy bản ghi học vấn đầu tiên
                            ?? new EducationExperienceRespone() // <-- THÊM DÒNG NÀY
                 })
                    .ToListAsync();
            return (data, total);
        }
        public async Task UpdateEmployeeAndReviewedDateAsync(int jobAppId, int employeeId)
        {
            var jobApp = await db.JobApplications.FindAsync(jobAppId);
            if (jobApp == null)
                throw new Exception("JobApplication not found");

            jobApp.EmployeeId = employeeId;
            db.JobApplications.Update(jobApp); // EF Core sẽ chỉ cập nhật các trường thay đổi
            await db.SaveChangesAsync();
        }
        // (Giả định JobApplicationRequest của bạn có một object EducationExperience
        // và object đó có một "Id" của bản ghi học vấn cần cập nhật)

        public async Task UpdateAsync(int Id, JobApplicationRequest dto)
        {
            // Dùng Transaction vì bạn cập nhật 2 bảng riêng biệt
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // === 1. CẬP NHẬT JOBAPPLICATION ===
                var job = await db.JobApplications.FindAsync(Id);
                if (job == null || job.IsDeleted)
                    throw new ArgumentException("Job posting not found");

                job.Skills = dto.Skills;
                job.Notes = dto.Notes;
                job.Status = dto.Status;
                job.ReviewedDate = DateTime.Now;

                if (dto.CvFilePath != null && dto.CvFilePath.Length > 0)
                {
                    job.CvFilePath = await EditCvAsync(dto.CvFilePath, dto.ExistingCvUrl);
                }
                else
                {
                    job.CvFilePath = dto.ExistingCvUrl;
                }
                if (dto.EducationExperience != null)
                {
                    // Tìm bản ghi học vấn bằng ID của chính nó
                    var eduExp = await db.educationExperiences.FindAsync(dto.EducationExperience.EducationExperienceId);

                    if (eduExp != null)
                    {
                        // Cập nhật các trường
                        eduExp.Major = dto.EducationExperience.Major;
                        eduExp.EducationLevel = dto.EducationExperience.EducationLevel;
                        eduExp.University = dto.EducationExperience.University;
                        eduExp.GPA = dto.EducationExperience.GPA;
                        eduExp.GraduationYear = dto.EducationExperience.GraduationYear;
                        eduExp.ExperienceDescription = dto.EducationExperience.ExperienceDescription;
                    }
                }

                // Lưu tất cả thay đổi (cho cả 2 bảng)
                await db.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }
        /// <summary>
        /// Lưu file CV mới và xóa file cũ nếu có.
        /// </summary>
        /// <param name="cvFile">File CV mới được upload</param>
        /// <param name="existingCvUrl">Đường dẫn CV cũ trong DB</param>
        /// <returns>Đường dẫn CV mới để lưu vào DB</returns>
        private async Task<string> EditCvAsync(IFormFile cvFile, string existingCvUrl)
        {
            if (cvFile == null || cvFile.Length == 0)
                return existingCvUrl; // Không upload file mới -> giữ file cũ

            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(existingCvUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCvUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Lưu file mới
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            return "/uploads/cvs/" + fileName; // trả về path mới để cập nhật DB
        }
        public async Task UpdateStatusAsync(int jobAppId, ApplicationStatus status)
        {
            var jobApp = await db.JobApplications.FindAsync(jobAppId);
            jobApp.Status = status;
            jobApp.ReviewedDate = DateTime.Now;
            await db.SaveChangesAsync();
        }
        public JobApplicationModel? SelectApplication(UserAccountModel account)
        {
            if (account.JobApplications == null || !account.JobApplications.Any())
                return null;

            return account.JobApplications
                .FirstOrDefault(j => j.Status == ApplicationStatus.Approved)
                    ?? account.JobApplications.FirstOrDefault();
        }
        public async Task<ApplicationDetailRespone> GetAppByEmployee(int applicationId)
        {
            return await db.JobApplications
                .Where(j => j.Id == applicationId && !j.IsDeleted)
                .Include(j => j.JobPosting)
                .Include(j => j.UserAccount)
                .ThenInclude(u => u.Employee)
                .Select(j => new ApplicationDetailRespone
                {
                    ApplicationId = j.Id,
                    JobTitle = j.JobPosting.Title,
                    SubmitAt = j.SubmittedDate,
                    SalaryRange = j.JobPosting.SalaryRange,
                    CvUrl = j.CvFilePath,
                    CandidateName = j.UserAccount.FullName,
                    CandidateEmail = j.UserAccount.Email,
                    CandidatePhone = j.UserAccount.Employee.Phone,
                    Status = j.Status
                })
                .FirstOrDefaultAsync() ?? new ApplicationDetailRespone();
        }

    }
}
