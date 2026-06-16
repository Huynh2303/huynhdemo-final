using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class OderManagementViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }

        public decimal Revenue { get; set; }

        public PaginatedList<OderViewModel>? Orders { get; set; } 
    }
}
