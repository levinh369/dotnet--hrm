namespace DACN.Enums
{
    public class StatusEnums
    {
        public enum UserRole
        {
            Admin = 0,
            Recruiter = 1,
            Employee = 2
        }

        public enum AccountStatus
        {
            Inactive = 0,
            Active = 1
        }

        public enum EmployeeStatus
        {
            ChuaNhanViec = 0,      // Đã tuyển nhưng chưa đi làm
            DangLam = 1,           // Đang làm việc
            TamNghi = 2,           // Tạm nghỉ
            DaNghi = 3             // Nghỉ hẳn
        }

        public enum ContractType
        {
            ThuViec = 0,
            CoThoiHan =1,
            ChinhThuc = 2,
            ThoiVu = 3,
        }
      
        public enum ContractStatus
        {
            HetHan = 0,
            ConHieuLuc = 1,
            ChuaHieuLuc = 2
        }

        public enum LeaveStatus
        {
            ChoDuyet = 0,
            Duyet = 1,
            TuChoi = 2
        }

        public enum NotificationType
        {
            NghiPhep = 0,
            CapNhatHoSo = 1,
            HopDongSapHetHan = 2,
            TuyenDungThanhCong= 3
        }
        public enum LeaveType
        {
            Annual = 0,       // Nghỉ phép năm
            Sick = 1,         // Nghỉ bệnh
            Unpaid = 2,       // Nghỉ không lương
            Maternity = 3,    // Nghỉ thai sản
            Other = 4         // Khác
        }
        public enum ReportType
        {
            Luong = 0,
            NhanVien = 1,
            HopDong = 2
        }
        public enum ActionType
        {
            Login = 0,          // Đăng nhập
            Logout = 1,         // Đăng xuất
            Create = 2,         // Thêm mới
            Update = 3,         // Cập nhật
            Delete = 4,         // Xóa
            Approve = 5,        // Duyệt
            Reject = 6,         // Từ chối
            View = 7,           // Xem dữ liệu
            Other = 8           // Hành động khác
        }
        public enum ApplicationStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }

        public enum JobPostingStatus
        {
            DangMo = 0,   // Đang mở tuyển dụng
            Dong = 1,     // Đóng (không nhận hồ sơ)
            HetHan = 2
        }
        public enum EmploymentType
        {
            Internship=0,
            FullTime=1,
            PartTime=2
        }
        public enum ExperienceLevel
        {
            None = 0,          // Không yêu cầu
            LessThan1Year = 1, // Dưới 1 năm
            OneToTwoYears = 2, // 1 - 2 năm
            ThreeOrMore = 3    // 3 năm trở lên
        }
        public enum EducationLevelEnum
        {
            TrungCap = 0,
            CaoDang = 1,
            CuNhan = 2,
            KySu = 3,
            ThacSi = 4,
            TienSi = 5
        }
        public enum AttendanceStatus
        {
            // Mặc định cái đầu là 0
            ChuaXacDinh = 0,

            DungGio = 1,     // On Time
            DiMuon = 2,      // Late
            VeSom = 3,       // Early Leave
            VangMat = 4,     // Absent
            NghiPhep = 5,    // Leave
            NgayLe = 6       // Holiday
        }
        public enum SalaryStatus
        {
            Draft = 0,      // Nháp (Mới tính, chưa chốt)
            Approved = 1,   // Đã chốt (Khóa sửa)
            Paid = 2,       // Đã thanh toán
            Cancelled = -1  // Hủy bỏ
        }
    }
}
