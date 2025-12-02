using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class NotificationRespone
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? EmployeeId { get; set; }
        public string? Url { get; set; }
    }
}
