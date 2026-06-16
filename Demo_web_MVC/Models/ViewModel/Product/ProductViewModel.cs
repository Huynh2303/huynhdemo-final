using Demo_web_MVC.Models.ViewModel.Category;
using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục sản phẩm")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
        public string? Brand { get; set; }
        public string? CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<ProductVariantsViewModel>? Variants { get; set; }
        public List<string> imageUrl { get; set; } = new List<string>();
        public List <CategoryViewModel>? Categories { get; set; }
        public List<ProductViewModel> RelatedProducts { get; set; } = new();

    }
}