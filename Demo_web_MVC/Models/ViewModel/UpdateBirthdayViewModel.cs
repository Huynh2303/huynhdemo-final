using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel
{
    public class UpdateBirthdayViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}
