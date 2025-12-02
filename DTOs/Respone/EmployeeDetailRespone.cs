using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class EmployeeDetailRespone
    {
        public int EmployeeId { get; set; }
        public int UserAccountId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CCCD { get; set; }
        public DateTime? Dob { get; set; }
        public bool Gender { get; set; }
        public string Address { get; set; }

        // ===== Công việc =====
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string PositionName { get; set; }
        public int PositionId { get; set; }
        public DateTime? StartDate { get; set; }
        public EmployeeStatus status { get; set; }

        // ===== Hồ sơ & Năng lực =====
        public int EducationExperienceId { get; set; }
        public EducationLevelEnum EducationLevel { get; set; }
        public string University { get; set; }
        public string Major { get; set; }
        public int? GraduationYear { get; set; }
        public decimal? GPA { get; set; }
        public string ExperienceDescription { get; set; }

        // ===== Lịch sử ứng tuyển =====
        public string AppliedJobTitle { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string CvFilePath { get; set; }
        public string RecruitNote { get; set; }
        public string Skills { get; set; }
        public string Note { get; set; }
       
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        // ===== Hợp đồng =====
        public List<ContractDTO>? Contracts { get; set; }
        public List<JobApplicationDto>? jobApplications { get; set; }
    }

    public class ContractDTO
    {
        public string? ContractCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal BaseSalary { get; set; }
        public ContractStatus Status { get; set; }
        public string? FilePath { get; set; }
        public string? Type { get; set; }
        public DateTime SignedDate { get; set; }
    }
    public class JobApplicationDto
    {
        public string? JobTitle { get; set; }
        public string? RecruiterName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string? CvFilePath { get; set; }
        public string? Skills { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }

}
