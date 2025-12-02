namespace DACN.DTOs.Respone
{
    public class DepartmentRespone
    {
        public int Id { get; set; }
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

    }
}
