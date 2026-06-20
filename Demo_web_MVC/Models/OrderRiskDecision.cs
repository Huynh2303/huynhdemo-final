namespace Demo_web_MVC.Models
{
    public class OrderRiskDecision
    {
        public string RiskLevel { get; set; } = "Low";

        public string Suggestion { get; set; } = "Có thể nhận đơn.";

        public List<string> Reasons { get; set; } = new();
        public float FinalScore { get; set; }
        public string WarningMessage { get; set; } = string.Empty;
    }
}
