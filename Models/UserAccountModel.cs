using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class UserAccountModel
    {
        [Key]
        public int UserAccountId { get; set; }  // PK riêng

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public string? PasswordHash { get; set; } = string.Empty;
        [StringLength(100)]
        public string? ActivationToken { get; set; } // Lưu mã token tại đây

        public DateTime? TokenExpiry { get; set; }   // Lưu thời hạn (ví dụ: 24h)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; }
        public UserRole Role { get; set; }
        public int? EmployeeId { get; set; }  

        [ForeignKey("EmployeeId")]
        public EmployeeModel? Employee { get; set; }
        public ICollection<JobApplicationModel>? JobApplications { get; set; }
        // 1 UserAccount có nhiều EducationExperience
        public ICollection<EducationExperienceModel>? EducationExperiences { get; set; }

    }

}
