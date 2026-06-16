using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class ProductsManagerViewModel
    {
        public PaginatedList<ProductViewModel> Products { get; set; } = null!;
    }
}
