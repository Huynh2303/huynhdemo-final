using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Token không được để trống")]
        public string? Token { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 đến 100 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu mới phải có ít nhất 1 chữ cái và 1 số")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}
