using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class ContractModel
    {
        [Key]
        public int ContractId { get; set; }

        // --- 1. ĐỊNH DANH HỢP ĐỒNG ---
        [Required]
        [StringLength(50)]
        public string ContractCode { get; set; } // VD: CNT-2025-001
        // --- 2. LIÊN KẾT NHÂN VIÊN ---
        [Required]
        public int EmployeeId { get; set; }
        public EmployeeModel? Employee { get; set; }

        // --- 3. THỜI GIAN (Quan trọng) ---
        public DateTime SignedDate { get; set; } // Ngày ký bút
        public DateTime StartDate { get; set; }  // Ngày bắt đầu hiệu lực
        public DateTime? EndDate { get; set; }   // Nullable: Để trống nếu là HĐ Vô thời hạn

        // --- 4. LƯƠNG & CHẾ ĐỘ (Quan trọng để tính lương) ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; } // Lương cơ bản/Lương thỏa thuận
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Allowance { get; set; } = 0;
        // --- 5. LƯU TRỮ ---
        [StringLength(500)]
        public string? FilePath { get; set; } // Đường dẫn file PDF đã upload

        // --- 6. PHÂN LOẠI & TRẠNG THÁI ---
        public ContractType Type { get; set; }   // Thử việc, Chính thức, CTV...
        public ContractStatus Status { get; set; } // Hiệu lực, Hết hạn, Đã hủy...

        [StringLength(500)]
        public string? Note { get; set; }

        // --- 7. SYSTEM FIELDS ---
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } // Thường dùng để đánh dấu "Đây là HĐ đang áp dụng hiện tại"
    }
}