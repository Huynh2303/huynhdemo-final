namespace Demo_web_MVC.Models.ViewModel
{
    public class FraudAnalysisViewModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal RiskScore { get; set; }

        public string RiskLevel { get; set; } = "";

        public string RiskReasons { get; set; } = "";

        public string ModelName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        // Danh sách lý do đã parse ra để hiển thị trong view
        public List<string> Reasons { get; set; } = new();
    }
}
