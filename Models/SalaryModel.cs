using DACN.Enums; // Nhớ using namespace chứa Enum
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    [Table("Salaries")]
    public class SalaryModel
    {
        [Key]
        public int SalaryId { get; set; }

        // 🔗 Liên kết nhân viên
        [Required]
        public int EmployeeId { get; set; } // Đổi UserId -> EmployeeId cho chuẩn

        [ForeignKey("EmployeeId")]
        public EmployeeModel? Employee { get; set; }

        // 📅 Thời gian
        [Required]
        public int Month { get; set; }
        [Required]
        public int Year { get; set; }

        // 📊 Căn cứ tính lương (Nên có để giải trình)
        public double StandardWorkDays { get; set; } = 26; 
        public double ActualWorkDays { get; set; }         // Công thực (lấy từ chấm công)

        // 💰 Các khoản tiền (Đã bỏ Bonus)
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } = 0;    // Lương cứng

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; } = 0;     // Phụ cấp (Ăn, xăng...)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deduction { get; set; } = 0;     // Khấu trừ (Đi muộn, BHXH)
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")]
        public decimal ManualDeduction { get; set; } = 0;

        // 📝 Trạng thái & Ghi chú
        public SalaryStatus Status { get; set; } = SalaryStatus.Draft; 

        [StringLength(500)]
        public string? Note { get; set; }

        // 🕒 System Info
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}