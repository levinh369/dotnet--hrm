using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class PositionModel
    {
        [Key]
        public int PositionId { get; set; }

        [Required, StringLength(100)]
        public string PositionName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<EmployeeModel>? Employees { get; set; }
    }
}
