using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class EmployeeRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int UserAccountId { get; set; }

        public int? EducationExperienceId { get; set; }

        // === TAB 1: Thông tin chung ===

        // --- Từ UserAccountModel ---
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        // --- Từ EmployeeModel ---
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? CCCD { get; set; }
        public string? Email { get; set; }
        public string? avatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string? Address { get; set; }

        [Required]
        public int? DepartmentId { get; set; }

        [Required]
        public int? PositionId { get; set; }

        public DateTime? StartDate { get; set; }
        public EmployeeStatus Status { get; set; }


        // === TAB 2: Hồ sơ & Năng lực ===

        // --- Từ EducationExperienceModel ---
        public EducationLevelEnum? EducationLevel { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public int? GraduationYear { get; set; }
        public string? Skills { get; set; }
        public string? ExperienceDescription { get; set; }
        public decimal? GPA { get; set; }
    }
}