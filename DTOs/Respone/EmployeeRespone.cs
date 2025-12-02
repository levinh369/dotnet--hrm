namespace DACN.DTOs.Respone
{
    public class EmployeeRespone
    {
        public int EmployeeId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Gender { get; set; }    
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? CCCD { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }   // ⭐ thêm

        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public int UserAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }


    }
}
