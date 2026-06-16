using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class DashboardViewModel
    {
            public int orderId { get; set; }
        public PaginatedList<OderViewModel> Orders { get; set; } = null!;
        public PaginatedList<ProductViewModel> Products { get; set; } = null!;

        
    }
}
