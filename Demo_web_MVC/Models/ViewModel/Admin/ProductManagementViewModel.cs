using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class ProductManagementViewModel
    {
        public int TotalProducts { get; set; }

        public int LowStockProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        public int TotalCategories { get; set; }

        public PaginatedList<ProductViewModel>? Products { get; set; }
    }
}
