using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SalaryModel> Salaries { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<PositionModel> Positions { get; set; }
        public DbSet<ContractModel> Contracts { get; set; }
        public DbSet<UserAccountModel> UserAccounts { get; set; }
        public DbSet<UserLogModel> UserLogs { get; set; }
        public DbSet<LeaveRequestModel> LeaveRequests { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<EducationExperienceModel> educationExperiences { get; set; }
        public DbSet<JobApplicationModel> JobApplications { get; set; }
        public DbSet<JobPostingModel> JobPosts { get; set; }
        public DbSet<AttendanceModel> Attendances { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ 1-1 Employee ↔ UserAccount
            modelBuilder.Entity<EmployeeModel>()
                .HasOne(e => e.Account)
                .WithOne() // UserAccount không cần back reference
                .HasForeignKey<EmployeeModel>(e => e.UserAccountId);
        }
    }
}
