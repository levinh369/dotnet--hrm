namespace DACN.DTOs.Respone
{
    public class AttendanceSummary
    {
        public DateTime Date { get; set; }
        public int TotalStaffCount { get; set; }
        public int OnTimeCount { get; set; }
        public int LateOrEarlyCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
