using Demo_web_MVC.Models.ViewModel.Product;
namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class ProductManagerDetailViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Brand { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public List<string> ProductImages { get; set; } = new();

        public List<ProductVariantsViewModel> Variants { get; set; } = new();

        public int TotalStock { get; set; }
        public int TotalVariants { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}
