using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class DepartmentModel
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; }
        public ICollection<EmployeeModel>? Employees { get; set; }
    }
}
