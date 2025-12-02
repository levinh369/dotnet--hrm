using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class LeaveRequestModel
    {
        
            [Key]
            public int Id { get; set; }

            [ForeignKey("Employee")]
            public int EmployeeId { get; set; }
            public EmployeeModel Employee { get; set; } = null!;

            [Required]
            public LeaveType Type { get; set; }  // Loại nghỉ phép

            [Required]
            public DateTime StartDate { get; set; }  // Ngày bắt đầu nghỉ

            [Required]
            public DateTime EndDate { get; set; }    // Ngày kết thúc nghỉ

            [StringLength(500)]
            public string? Reason { get; set; }      // Lý do nghỉ

            public LeaveStatus Status { get; set; } = LeaveStatus.ChoDuyet; // Trạng thái duyệt

            [StringLength(500)]
            public string? ManagerComment { get; set; } // Ghi chú của người duyệt (nếu có)

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }
            public bool IsDeleted { get; set; } = false;
        
    }
}
