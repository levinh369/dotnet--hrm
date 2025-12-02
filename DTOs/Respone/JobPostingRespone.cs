using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class JobPostingRespone
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? CreateName { get; set; }
        public string? JobDescription { get; set; }
        public string? SalaryRange { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? UpdateAt { get; set; }
        public JobPostingStatus Status { get; set; }
        public bool IsActive { get; set; }  
        public string? Requirements {  get; set; }
        public bool? IsDeleted { get; set; }
        public string StatusText => Status switch
        {
            JobPostingStatus.DangMo => "Đang mở",
            JobPostingStatus.Dong => "Đóng",
            JobPostingStatus.HetHan => "Hết hạn",
            _ => "Không xác định"
        };
        public EmploymentType EmploymentType { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
        public string? positionName { get; set; }
        public int? ViewCount { get; set; }
        public int PositionId { get; set; }
        public int DepartmentId { get; set; }

    }
}
