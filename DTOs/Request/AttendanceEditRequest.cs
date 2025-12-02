using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class AttendanceEditRequest
    {
        public DateTime EditTime { get; set; }
        public TimeSpan? RawCheckInTime { get; set; }
        public TimeSpan? RawCheckOutTime { get; set; }
        public string? EditedBy { get; set; }
        public string? EditReason { get; set; }
    }
}
