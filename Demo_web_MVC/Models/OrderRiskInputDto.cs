namespace Demo_web_MVC.Models
{
    public class OrderRiskInputDto
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public int AccountAgeDays { get; set; }

        public int TotalOrders { get; set; }

        public int OrdersLast24h { get; set; }

        public int OrdersLast7d { get; set; }

        public int CancelledOrders { get; set; }

        public decimal CancelRate { get; set; }

        public decimal CurrentOrderValue { get; set; }

        public decimal AvgOrderValue { get; set; }

        public int IsCod { get; set; }

        public int CodOrderCount { get; set; }

        public int PhoneUsedCount { get; set; }

        public int AddressUsedCount { get; set; }

        public int ItemCount { get; set; }

        public int TotalQuantity { get; set; }

        public int StatusChangeCount { get; set; }
        public int CancelledOrdersLast24h { get; set; }

        public decimal CancelRateLast24h { get; set; }

        public int CancelledOrdersLast7d { get; set; }

        public decimal CancelRateLast7d { get; set; }
        public int IsVip { get; set; }

        public int CompletedOrderCount { get; set; }

        public decimal CompletionRate { get; set; }
    }
}