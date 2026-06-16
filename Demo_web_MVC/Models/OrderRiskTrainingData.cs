using Microsoft.ML.Data;

namespace Demo_web_MVC.Models
{
    public class OrderRiskTrainingData
    {
        [LoadColumn(0)]
        public float AccountAgeDays { get; set; }

        [LoadColumn(1)]
        public float TotalOrders { get; set; }

        [LoadColumn(2)]
        public float OrdersLast24h { get; set; }

        [LoadColumn(3)]
        public float OrdersLast7d { get; set; }

        [LoadColumn(4)]
        public float CancelledOrders { get; set; }

        [LoadColumn(5)]
        public float CancelRate { get; set; }

        [LoadColumn(6)]
        public float CurrentOrderValue { get; set; }

        [LoadColumn(7)]
        public float AvgOrderValue { get; set; }

        [LoadColumn(8)]
        public float IsCod { get; set; }

        [LoadColumn(9)]
        public float CodOrderCount { get; set; }

        [LoadColumn(10)]
        public float PhoneUsedCount { get; set; }

        [LoadColumn(11)]
        public float AddressUsedCount { get; set; }

        [LoadColumn(12)]
        public float ItemCount { get; set; }

        [LoadColumn(13)]
        public float TotalQuantity { get; set; }

        [LoadColumn(14)]
        public float StatusChangeCount { get; set; }

        [LoadColumn(15)]
        public float CancelledOrdersLast24h { get; set; }

        [LoadColumn(16)]
        public float CancelRateLast24h { get; set; }

        [LoadColumn(17)]
        public float CancelledOrdersLast7d { get; set; }

        [LoadColumn(18)]
        public float CancelRateLast7d { get; set; }

        [LoadColumn(19)]
        [ColumnName("Label")]
        public bool IsRisk { get; set; }
    }
}
