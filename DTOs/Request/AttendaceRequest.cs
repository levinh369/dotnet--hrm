using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class AttendaceRequest
    {
        public int EmployeeId { get; set; }
        public string? Note { get; set; }
        public DateTime EditTime { get; set; }
        public TimeSpan? RawCheckInTime { get; set; }
        public TimeSpan? RawCheckOutTime { get; set; }
        public string? EditedBy { get; set; }
        public string? EditReason { get; set; }
    }
}
