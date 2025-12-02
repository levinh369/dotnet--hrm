using DACN.Models;
using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class JobPostingModel
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SalaryRange { get; set; }

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;
        public DateTime? ExpirationDate { get; set; }
        public DateTime? UpdateAt { get; set; }
        public JobPostingStatus Status { get; set; } = JobPostingStatus.DangMo;

        // Foreign keys
        public int? DepartmentId { get; set; }
        public DepartmentModel? Department { get; set; }
        public int? PositionId { get; set; }       // FK
        public PositionModel? Position { get; set; } // navigation

        public int? CreatedById { get; set; }       // FK
        public EmployeeModel? CreatedBy { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public ExperienceLevel RequiredExperience { get; set; }

        public int? ViewCount { get; set; } = 0;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        // Navigation
        public ICollection<JobApplicationModel>? Applications { get; set; }
    }
}
