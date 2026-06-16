using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập hoặc email không được vượt quá 100 ký tự")]
        public string? UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
        public string? Password { get; set; }
    }
}
