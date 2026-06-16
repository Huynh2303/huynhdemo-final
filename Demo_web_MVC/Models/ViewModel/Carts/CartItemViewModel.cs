using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Carts
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }

        public string? ProductName { get; set; }
        public string ?Brand { get; set; }
        public string ?Size { get; set; }
        public string ?Color { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        
    }
}
