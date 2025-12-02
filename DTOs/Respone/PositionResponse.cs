namespace DACN.DTOs.Respone
{
    public class PositionResponse
    {
        public int Id { get; set; }
        public string? PositionName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
