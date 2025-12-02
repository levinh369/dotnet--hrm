using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class EducationExperienceRequest
    {
        public int EducationExperienceId { get; set; } // Dùng để cập nhật nếu cần  
        public EducationLevelEnum EducationLevel { get; set; }
        public string? Major { get; set; }
        public string? University { get; set; }
        public int? GraduationYear { get; set; }
        public decimal? GPA { get; set; }
        public string? ExperienceDescription { get; set; }
    }
}
