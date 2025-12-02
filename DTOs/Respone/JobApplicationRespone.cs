using DACN.DTOs.Request;
using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class JobApplicationRespone
    {
        public string CandidateName { get; set; } = string.Empty;
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public int JobPostingId { get; set; }
        public EmploymentType AppliedType { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public ApplicationStatus Status { get; set; }

        public string? Skills { get; set; }
        public string ? CvUrl { get; set; }
        public string? Notes { get; set; }
        public List<JobPostingRespone> JobPostings { get; set; } = new List<JobPostingRespone>();


        // Dữ liệu bảng EducationExperience
        public EducationExperienceRespone EducationExperience { get; set; } = new EducationExperienceRespone();
    }
}
