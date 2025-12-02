using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class NotificationModel
    {
        [Key]
        public int NotificationId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Content { get; set; }

        public int? EmployeeId { get; set; } // null = toàn hệ thống nhân viên
        public EmployeeModel? Employee { get; set; }

        public int? UserId { get; set; } // null = toàn hệ thống user khác (Hiring, Admin, v.v.)
        public UserAccountModel? User { get; set; }

        public NotificationType Type { get; set; }
        public string Url { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
