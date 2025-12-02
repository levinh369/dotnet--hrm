using DACN.Data;
using DACN.DTOs.Respone;
using DACN.DTOs.Request;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{

    public class JobPostingRepository
    {
        private readonly ApplicationDbContext db;

        public JobPostingRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task CreateJobPostingAsync(JobPostingRequest dto)
        {
            var job = new JobPostingModel
            {
                Title = dto.Title,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                SalaryRange = dto.SalaryRange,
                PostedDate = dto.PostedDate,
                ExpirationDate = dto.ExpirationDate,
                JobDescription = dto.JobDescription,
                IsActive = true,
                Status = (JobPostingStatus)dto.status,
                CreatedById = dto.CreateById,
                Requirements = dto.Requirements,
                EmploymentType = (EmploymentType)dto.EmploymentType,
                RequiredExperience = (ExperienceLevel)dto.ExperienceLevel
            };

            db.JobPosts.Add(job);
            await db.SaveChangesAsync();
        }

        public IQueryable<JobPostingModel> QueryAll()
        {
            return db.JobPosts
                .AsNoTracking()
                .Include(j => j.Department)
                .Include(j => j.Position)
                .Include(j => j.CreatedBy)
                    .ThenInclude(a => a.Account);
        }
        public async Task<(List<JobPostingModel> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, DateTime? toDate, int status, int DepartMentId, DateTime? ExpirationDate)
        {
            var query = db.JobPosts
                .Include(j => j.Department)
                .Include(j => j.Position)
                .Include(j => j.CreatedBy)
                    .ThenInclude(e => e.Account)
                 .Where(j=>!j.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(j => j.Title.Contains(keySearch));

            if (fromDate.HasValue)
                query = query.Where(j => j.PostedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(j => j.PostedDate <= toDate.Value);
            if (ExpirationDate.HasValue)
                query = query.Where(j => j.ExpirationDate <= ExpirationDate.Value);
            if (status != -1)
                query = query.Where(j => (int)j.Status == status);
            if (DepartMentId != 0)
                query = query.Where(j => j.DepartmentId == DepartMentId);
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(j => j.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task<(List<JobPostingModel> Data, int Total)> SearchJobs(
        int page, int pageSize, string keySearch)
        {
            var query = db.JobPosts
                .Include(j => j.Department)
                .Include(j => j.Position)
                .Include(j => j.CreatedBy)
                    .ThenInclude(e => e.Account)
                 .Where(j => !j.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(j => j.Title.Contains(keySearch));
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(j => j.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task<JobPostingModel?> GetByIdAsync(int id)
        {
            return await db.JobPosts
                .Include(j => j.Department) // load navigation property nếu cần
                .Include(j => j.Position)
                .Include(j => j.CreatedBy)
                 .ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(j => j.Id == id && j.IsActive && !j.IsDeleted);
        }
        public async Task<List<JobPostingModel>> GetAllActiveAsync()
        {
            return await db.JobPosts
                .Include(j => j.Department)
                .Include(j => j.Position)
                .Where(j => j.IsDeleted == false
                            && j.IsActive == true
                            && j.Status == JobPostingStatus.DangMo)
                .OrderByDescending(j => j.ViewCount)       // 🔥 nhiều lượt xem nhất trước
                .ThenByDescending(j => j.PostedDate)       // 🔥 sau đó mới tới bài mới nhất
                .ToListAsync();
        }
        public async Task UpdateJobPostingAsync(JobPostingRequest dto)
        {
            var job = await db.JobPosts.FindAsync(dto.Id);
            if (job == null || job.IsDeleted)
                throw new ArgumentException("Job posting not found");
            job.Title = dto.Title;
            job.DepartmentId = dto.DepartmentId;
            job.PositionId = dto.PositionId;
            job.SalaryRange = dto.SalaryRange;
            job.PostedDate = dto.PostedDate;
            job.ExpirationDate = dto.ExpirationDate;
            job.JobDescription = dto.JobDescription;
            job.UpdateAt = DateTime.Now;
            job.Status = (JobPostingStatus)dto.status;
            job.Requirements = dto.Requirements;
            job.EmploymentType = (EmploymentType)dto.EmploymentType;
            job.RequiredExperience = (ExperienceLevel)dto.ExperienceLevel;
            await db.SaveChangesAsync();
        }
        public async Task DeleteJobPostingAsync(int id)
        {
            var job = await db.JobPosts.FindAsync(id);
            if (job == null || job.IsDeleted)
                throw new ArgumentException("Job posting not found or already deleted");

            job.IsDeleted = true;
            await db.SaveChangesAsync();
        }
        public async Task<JobPostingModel?> GetJobByIdAsync(int jobId)
        {
            return await db.JobPosts
                .FirstOrDefaultAsync(j => j.Id == jobId && !j.IsDeleted);
        }


    }
}
