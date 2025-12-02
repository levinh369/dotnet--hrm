using Microsoft.AspNetCore.Builder;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class EducationExperienceModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserAccountId { get; set; }
        public UserAccountModel UserAccount { get; set; }

        [Required, StringLength(100)]
        public EducationLevelEnum EducationLevel { get; set; }

        [StringLength(150)]
        public string? Major { get; set; }

        [StringLength(150)]
        public string? University { get; set; }

        public int? GraduationYear { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? GPA { get; set; }

        public string? ExperienceDescription { get; set; } // Work experience summary

        // Navigation
       
    }
}
