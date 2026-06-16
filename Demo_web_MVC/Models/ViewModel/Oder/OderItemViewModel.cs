using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Oder
{
    public class OderItemViewModel
    {

        public int OrderId { get; set; }

        public int VariantId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
        
        public string? Name { get; set; }

       public string? Img { get; set; }
        public int? SellerId { get; set; }
        public ProductVariant Variant { get; set; } = new ProductVariant();
    }
}
