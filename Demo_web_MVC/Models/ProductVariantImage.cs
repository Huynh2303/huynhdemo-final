using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models
{
    public class ProductVariantImage
    {
        public int Id { get; set; }

        public int VariantId { get; set; } // FK

        public string Url { get; set; } = null!;

        public int SortOrder { get; set; }

        // Navigation property
        public ProductVariant Variant { get; set; } = null!;
        
    }
}
