using DACN.Areas.Admin.Controllers;
using DACN.Data;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Enums;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class AttendanceRepository
    {
        private readonly ApplicationDbContext db;

        public AttendanceRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        // Input: ID nhân viên (lấy từ Token/Session) và DTO chứa Ghi chú
        public async Task<AttendanceModel?> GetTodayAttendanceAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await db.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate == today);
        }
        public async Task<AttendanceModel?> GetByIdAsync(int id)
        {
            return await db.Attendances
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Account)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Department)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Position)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<(List<AttendanceModel> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, int? departmentId)
        {
            // 1. Xác định ngày cần xem (Mặc định hôm nay nếu null)
            var targetDate = fromDate?.Date ?? DateTime.Now.Date;

            // 2. Bắt đầu query từ bảng NHÂN VIÊN (Để lấy được cả người vắng)
            var query = db.Employees
                .Include(e => e.Account)
                .Include(e => e.Department)
                .Where(e => e.Status == EmployeeStatus.DangLam) // Chỉ lấy nhân viên đang hoạt động
                .AsNoTracking()
                .AsQueryable();

            // 3. Lọc theo điều kiện (Tên, Mã, Phòng ban)
            if (!string.IsNullOrEmpty(keySearch))
            {
                query = query.Where(e => e.Account.FullName.Contains(keySearch) || e.EmployeeId.ToString().Contains(keySearch));
            }
            if (departmentId > 0)
            {
                query = query.Where(e => e.DepartmentId == departmentId);
            }

            // 4. Đếm tổng số nhân viên (để phân trang)
            int total = await query.CountAsync();

            // 5. Lấy danh sách nhân viên của trang hiện tại (VD: 10 người)
            var employees = await query
                .OrderBy(e => e.EmployeeId) // Sắp xếp theo ID hoặc Tên
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Lấy dữ liệu chấm công của những nhân viên này trong ngày targetDate
            var empIds = employees.Select(e => e.EmployeeId).ToList();
            var attendances = await db.Attendances
                .Where(a => empIds.Contains(a.EmployeeId) && a.WorkDate == targetDate)
                .ToListAsync();

            // 7. GỘP DỮ LIỆU (Mapping thủ công)
            var resultList = new List<AttendanceModel>();

            foreach (var emp in employees)
            {
                // Tìm xem nhân viên này có bản ghi chấm công hôm nay không?
                var att = attendances.FirstOrDefault(a => a.EmployeeId == emp.EmployeeId);

                if (att != null)
                {
                    // TRƯỜNG HỢP 1: CÓ ĐI LÀM (Lấy bản ghi thật từ DB)
                    att.Employee = emp; // Gán lại object Employee để View hiển thị tên
                    resultList.Add(att);
                }
                else
                {
                    // TRƯỜNG HỢP 2: VẮNG MẶT (Tạo bản ghi giả lập)
                    // Vì chưa chấm công nên chưa có trong DB -> Tạo ra object tạm để hiển thị
                    var absentRecord = new AttendanceModel
                    {
                        Id = 0, // ID = 0 báo hiệu chưa lưu DB
                        EmployeeId = emp.EmployeeId,
                        Employee = emp, // Gán thông tin nhân viên để hiện tên
                        WorkDate = targetDate,

                        // Set các giá trị mặc định cho người vắng
                        CheckInTime = null,
                        CheckOutTime = null,
                        RawCheckInTime = null,
                        RawCheckOutTime = null,
                        WorkingHours = 0,

                        // Quan trọng: Gán Status là Vắng Mặt
                        Status = AttendanceStatus.VangMat,
                        Notes = "Chưa có dữ liệu chấm công"
                    };
                    resultList.Add(absentRecord);
                }
            }

            return (resultList, total);
        }
        public async Task CheckInAsync(AttendaceRequest dto)
        {
            var today = DateTime.Today;
            var exists = await db.Attendances
                .AnyAsync(a => a.EmployeeId == dto.EmployeeId && a.WorkDate == today);

            if (exists)
            {
                throw new Exception("Hôm nay bạn đã Check-in rồi!");
            }

            var now = DateTime.Now;
            var checkInTime = now.TimeOfDay;
            // 3. Xác định trạng thái (Giả sử 8:00 là giờ vào làm)
            var startWorkTime = new TimeSpan(8, 0, 0);
            AttendanceStatus calculatedStatus = (checkInTime <= startWorkTime)
            ? AttendanceStatus.DungGio
            : AttendanceStatus.DiMuon;

            // 4. Tạo bản ghi Chấm công
            var attendance = new AttendanceModel
            {
                EmployeeId = dto.EmployeeId,
                WorkDate = today,          // Ngày 
                CheckInTime = checkInTime, // Giờ vào
                CheckOutTime = null,       // Giờ ra để null
                Status = calculatedStatus,           // Trạng thái tính toán ở trên
                Notes = dto.Note,          // Ghi chú từ Frontend
                WorkingHours = 0           // Chưa có giờ ra nên công = 0
            };

            db.Attendances.Add(attendance);
            await db.SaveChangesAsync();
        }
        public async Task CheckOutAsync(AttendaceRequest dto)
        {
            var today = DateTime.Today;

            // 1. Tìm bản ghi Check-in của nhân viên trong ngày hôm nay
            var attendance = await db.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.WorkDate == today);

            // --- VALIDATION ---
            if (attendance == null)
            {
                // Chưa Check-in thì không thể Check-out
                throw new Exception("Bạn chưa Check-in hôm nay, không thể thực hiện Check-out!");
            }

            if (attendance.CheckOutTime != null)
            {
                // Đã Check-out rồi thì không được bấm nữa (Tránh spam)
                throw new Exception("Bạn đã hoàn thành Check-out rồi!");
            }

            // 2. Lấy giờ hiện tại
            var now = DateTime.Now;
            var checkOutTime = now.TimeOfDay;

            // 3. Tính toán Số giờ làm việc (Giờ ra - Giờ vào)
            // CheckInTime là nullable nên cần check HasValue cho chắc chắn
            if (attendance.CheckInTime.HasValue)
            {
                TimeSpan duration = checkOutTime - attendance.CheckInTime.Value;
                attendance.WorkingHours = Math.Round(duration.TotalHours, 2); // Làm tròn 2 số thập phân (VD: 8.5 giờ)
            }

            // 4. Xác định trạng thái Về Sớm
            // Giả sử quy định giờ tan làm là 17:00 (5 PM)
            var endWorkTime = new TimeSpan(17, 0, 0);

            if (checkOutTime < endWorkTime)
            {
                attendance.Status = AttendanceStatus.VeSom;
            }
            attendance.CheckOutTime = checkOutTime;
            if (!string.IsNullOrEmpty(dto.Note))
            {
                attendance.Notes = string.IsNullOrEmpty(attendance.Notes)
                    ? dto.Note
                    : $"{attendance.Notes} | Out: {dto.Note}";
            }
            db.Attendances.Update(attendance);
            await db.SaveChangesAsync();
        }
        public async Task<AttendanceSummary> GetAttendanceSummaryAsync(DateTime? dateFilter)
        {
            var targetDate = dateFilter?.Date ?? DateTime.Now.Date;

            var startWorkTime = new TimeSpan(8, 0, 0);
            var endWorkTime = new TimeSpan(17, 0, 0);

            var totalAll = await db.Employees.CountAsync(e => e.Status == EmployeeStatus.DangLam);
            var dailyAttendanceQuery = db.Attendances.Where(c => c.WorkDate == targetDate);

            // --- 1. ĐẾM NGƯỜI CÓ VẤN ĐỀ (ĐI MUỘN HOẶC VỀ SỚM) ---
            // Logic: Chỉ cần vi phạm 1 trong 2 điều kiện là bị tính vào đây
            var lateOrEarlyCount = await dailyAttendanceQuery
                .CountAsync(c =>
                    // Điều kiện 1: Đi muộn
                    ((c.RawCheckInTime ?? c.CheckInTime) > startWorkTime)
                    ||
                    // Điều kiện 2: Về sớm (Chỉ tính khi đã có CheckOut)
                    ((c.RawCheckOutTime ?? c.CheckOutTime) != null &&
                     (c.RawCheckOutTime ?? c.CheckOutTime) < endWorkTime)
                );

            // --- 2. ĐẾM NGƯỜI ĐÚNG GIỜ (HOÀN HẢO) ---
            // Logic: Phải thõa mãn CẢ 2 điều kiện (Đến đúng VÀ (Chưa về hoặc Về đúng))
            var onTimeCount = await dailyAttendanceQuery
                .CountAsync(c =>
                    // Điều kiện 1: Có checkin VÀ Checkin đúng giờ
                    (c.RawCheckInTime ?? c.CheckInTime) != null &&
                    (c.RawCheckInTime ?? c.CheckInTime) <= startWorkTime
                    &&
                    // Điều kiện 2: (Chưa Checkout) HOẶC (Checkout đúng giờ)
                    // Phải thêm đoạn này để loại trừ những ông về sớm ra khỏi nhóm Đúng giờ
                    (
                        (c.RawCheckOutTime ?? c.CheckOutTime) == null ||
                        (c.RawCheckOutTime ?? c.CheckOutTime) >= endWorkTime
                    )
                );

            var presentCount = await dailyAttendanceQuery
                .CountAsync(c => (c.RawCheckInTime ?? c.CheckInTime) != null);

            var absentCount = totalAll - presentCount;

            return new AttendanceSummary
            {
                TotalStaffCount = totalAll,
                OnTimeCount = onTimeCount,
                LateOrEarlyCount = lateOrEarlyCount,
                AbsentCount = Math.Max(0, absentCount)
            };
        }
        public async Task UpdateAttendancetAsync(int id, AttendaceRequest dto)
        {
            var att = await db.Attendances.FindAsync(id);
            if (att == null)
                throw new ArgumentException("Attendance not found");
            att.EditReason = dto.EditReason;
            att.RawCheckInTime = dto.RawCheckInTime;
            att.RawCheckOutTime = dto.RawCheckOutTime;
            att.EditedBy = dto.EditedBy;
            att.EditTime = DateTime.Now;
            if (att.RawCheckInTime.HasValue && att.RawCheckOutTime.HasValue)
            {
                var duration = att.RawCheckOutTime.Value - att.RawCheckInTime.Value;
                if (duration.TotalMinutes < 0)
                {
                    duration = duration.Add(TimeSpan.FromDays(1));
                }

                att.WorkingHours = Math.Round(duration.TotalHours, 2);
            }
            else
            {
                att.WorkingHours = 0;
            }
            TimeSpan startShift = new TimeSpan(8, 0, 0);
            if (att.RawCheckInTime.HasValue && att.RawCheckInTime.Value > startShift)
            {
                att.Status = AttendanceStatus.DiMuon;
            }
            else
            {
                att.Status = AttendanceStatus.DungGio;
            }
            await db.SaveChangesAsync();
        }
    }
}
