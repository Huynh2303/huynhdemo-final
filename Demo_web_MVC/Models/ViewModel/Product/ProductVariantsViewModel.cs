using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel.Product
{
    public class ProductVariantsViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kích thước")]
        [StringLength(50, ErrorMessage = "Kích thước không được vượt quá 50 ký tự")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu sắc")]
        [StringLength(50, ErrorMessage = "Màu sắc không được vượt quá 50 ký tự")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(typeof(decimal), "1000", "999999999", ErrorMessage = "Giá bán phải từ 1.000đ trở lên")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int Stock { get; set; }

        public List<string> ImageUrlsVariants { get; set; } = new List<string>();

        public List<IFormFile>? ImageFiles { get; set; }
    }
}