using Microsoft.ML.Data;

namespace Demo_web_MVC.Models
{
    public class OrderRiskPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsRisk { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
