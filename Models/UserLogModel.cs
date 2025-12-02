using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DACN.Enums.StatusEnums;

namespace DACN.Models
{
    public class UserLogModel
    {
            [Key]
            public int Id { get; set; }

            [ForeignKey("UserAccountModel")]
            public int UserAccountId { get; set; }
            public UserAccountModel UserAccount { get; set; } = null!;

            [Required]
            public ActionType ActionType { get; set; }   // Loại hành động

            [StringLength(200)]
            public string? TableName { get; set; }       // Tên bảng tác động (VD: "Employee", "LeaveRequest", ...)

            [StringLength(200)]
            public string? RecordId { get; set; }        // Khóa chính của bản ghi bị tác động (nếu có)

            [StringLength(1000)]
            public string? Description { get; set; }     // Mô tả hành động (VD: "Cập nhật thông tin nhân viên #12")

            public string? IPAddress { get; set; }       // Địa chỉ IP của người thực hiện

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }
            public bool IsDeleted { get; set; } = false;
        
    }
}
