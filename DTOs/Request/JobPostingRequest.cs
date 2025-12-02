namespace DACN.DTOs.Request
{
    public class JobPostingRequest
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public string? SalaryRange { get; set; }
        public string? Requirements { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? JobDescription { get; set; }
        public int CreateById { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsActive { get;set; }
        public int status { get;set; }
        public int EmploymentType { get; set; }
        public int ExperienceLevel { get; set; }


    }
}
