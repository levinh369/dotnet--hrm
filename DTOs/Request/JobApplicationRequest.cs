using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class JobApplicationRequest
    {
        public int EmployeeId { get; set; }
        public int UserAccountId { get; set; }
        public int JobPostingId { get; set; }
        public EmploymentType AppliedType { get; set; }
        public string? Skills { get; set; }
        public IFormFile? CvFilePath { get; set; }
        public string? ExistingCvUrl{ get; set; }
        public string? Notes { get; set; }
        public ApplicationStatus Status { get; set; }

        // Dữ liệu bảng EducationExperience
        public EducationExperienceRequest EducationExperience { get; set; } = new EducationExperienceRequest();
    }
}
