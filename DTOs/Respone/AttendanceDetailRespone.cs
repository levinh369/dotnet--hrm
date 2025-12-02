using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class AttendanceDetailRespone
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Avatar { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? avatarUrl { get; set; }
        public DateTime? WorkDate { get; set; }

        // Thông tin chấm công chi tiết
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public double WorkingHours { get; set; }

        // Trạng thái & Ghi chú
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public bool IsLate { get; set; }
        public bool IsLeftEarly { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public bool IsCheckInEdited { get; set; }
        public bool IsCheckOutEdited { get; set; }
        public TimeSpan? RawCheckIn { get; set; }
        public TimeSpan? RawCheckOut { get; set; }
        public string? EditReason { get; set; }
        public string? EditedBy { get; set; }
        public bool? isCheckInEdited { get;set; }
        public bool? isCheckOutEdited { get;set;}
    }
}
