using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class StatisticsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }

        // Doanh thu theo ngày trong tuần
        public Dictionary<DateTime, decimal> RevenuePerDay { get; set; } = new();

        // Doanh thu theo 7 ngày gần nhất
        public Dictionary<DateTime, decimal> RevenueLast7Days { get; set; } = new();

        // Doanh thu theo 30 ngày gần nhất (1 tháng)
        public Dictionary<DateTime, decimal> RevenueLast30Days { get; set; } = new();

        public Dictionary<OrderStatus, int> OrderStatusAll { get; set; } = new();
        public Dictionary<OrderStatus, int> OrderStatusLast7Days { get; set; } = new();
        public Dictionary<OrderStatus, int> OrderStatusLast30Days { get; set; } = new();
        public List<OderViewModel> Orders { get; set; } = new List<OderViewModel>();
        public DateTime CreatedAt { get; set; } // ngày giờ order
        public decimal TotalAmount { get; set; } // tổng tiền từng đơn

    }
}
