using System.ComponentModel.DataAnnotations;

namespace DACN.DTOs.Request
{
    public class ActivateAccountRequest
    {
        [Required]
        public string Token { get; set; } // Để biết đang kích hoạt cho ai

        public string? Email { get; set; } // Để hiển thị cho user biết (readonly)

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
