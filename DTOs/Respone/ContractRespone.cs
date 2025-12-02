using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class ContractRespone
    {
        public int ContractId { get; set; }
        public string? ContractCode { get; set; } // VD: CNT-2025-001
        public string? ContractCodeNew { get; set; } // Mã hợp đồng mới nếu có thay đổi
        public string? Name { get; set; } // Tên gợi nhớ (VD: HĐLĐ Chính thức 2025)
        public int EmployeeId { get; set; }
        public DateTime SignedDate { get; set; } // Ngày ký bút
        public DateTime StartDate { get; set; }  // Ngày bắt đầu hiệu lực
        public DateTime? EndDate { get; set; }   // Nullable: Để trống nếu là HĐ Vô thời hạn
        public decimal BasicSalary { get; set; } // Lương cơ bản/Lương thỏa thuận
        public string? FilePath { get; set; } // Đường dẫn file PDF đã upload
        public int Type { get; set; }   // Thử việc, Chính thức, CTV...
        public ContractStatus Status { get; set; } // Hiệu lực, Hết hạn, Đã hủy...
        public string? Note { get; set; }
        public ContractType ContractType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; } // Thường dùng để đánh dấu "Đây là HĐ đang áp dụng hiện tại"
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
