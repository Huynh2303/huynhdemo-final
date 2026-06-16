using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class AdminViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }

        public List<OderViewModel> oderViewModels { get; set; } = new();
    }
}
