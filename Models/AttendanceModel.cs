using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class AttendanceModel
    {
        [Key]
        public int Id { get; set; }

        // 1. AI CHẤM CÔNG?
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public EmployeeModel? Employee { get; set; }

        // 2. NGÀY NÀO?
        [Required]
        public DateTime WorkDate { get; set; } // Lưu ngày (VD: 2025-11-22 00:00:00)

        public TimeSpan? CheckInTime { get; set; }  // Lưu giờ vào (VD: 08:05:00)
        public TimeSpan? CheckOutTime { get; set; } // Lưu giờ ra (VD: 17:30:00)
        public TimeSpan? RawCheckInTime { get; set; }
        public TimeSpan? RawCheckOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public double? WorkingHours { get; set; }
        [StringLength(255)]
        public string? EditReason { get; set; }
        [StringLength(100)]
        public string? EditedBy { get; set; }
        public DateTime? EditTime { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
