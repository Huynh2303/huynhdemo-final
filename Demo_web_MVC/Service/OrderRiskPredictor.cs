using Demo_web_MVC.Models;
using Microsoft.ML;

namespace Demo_web_MVC.Service
{
    public class OrderRiskPredictor
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public OrderRiskPredictor()
        {
            _mlContext = new MLContext();

            var modelPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "MLModels",
                "order_risk_model.zip"
            );

            _model = _mlContext.Model.Load(modelPath, out _);
        }

        public OrderRiskPrediction Predict(OrderRiskTrainingData input)
        {
            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<OrderRiskTrainingData, OrderRiskPrediction>(_model);

            return predictionEngine.Predict(input);
        }

        public string GetRiskLevel(OrderRiskTrainingData input, OrderRiskPrediction prediction)
        {
            return GetRiskDecision(input, prediction).RiskLevel;
        }

        public string GetSuggestion(string riskLevel)
        {
            if (riskLevel == "DataWarning")
            {
                return "Đơn thiếu dữ liệu sản phẩm hoặc tổng tiền, cần kiểm tra lại dữ liệu đơn hàng.";
            }

            if (riskLevel == "High")
            {
                return "Nên gọi xác nhận khách hàng trước khi chuyển đơn sang Shipping.";
            }

            if (riskLevel == "Medium")
            {
                return "Nên kiểm tra lại thông tin khách hàng trước khi nhận đơn.";
            }

            return "Có thể nhận đơn.";
        }

        public OrderRiskDecision GetRiskDecision(OrderRiskTrainingData input, OrderRiskPrediction prediction)
        {
            if (input.CurrentOrderValue <= 0 ||
                input.ItemCount <= 0 ||
                input.TotalQuantity <= 0)
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "DataWarning",
                    WarningMessage = "Đơn hàng thiếu dữ liệu sản phẩm, tổng tiền hoặc số lượng nên hệ thống chưa thể đánh giá rủi ro chính xác.",
                    Suggestion = GetSuggestion("DataWarning"),
                    FinalScore = 0,
                    Reasons = new List<string>
            {
                "Dữ liệu đơn hàng: Đơn thiếu dữ liệu sản phẩm, tổng tiền hoặc số lượng."
            }
                };
            }

            var reasonScores = new List<(string Field, float Impact, string Message)>();

            void AddReason(string field, float impact, string message)
            {
                reasonScores.Add((field, impact, message));
            }

            // Không dùng prediction.IsRisk.
            // Chỉ lấy Score của AI làm điểm nền.
            float aiBaseScore = NormalizeAiScore(prediction.Score);
            float finalScore = aiBaseScore;

            // =========================
            // 1. Điểm bảo vệ khách tốt
            // Các lý do giảm điểm vẫn dùng để tính toán,
            // nhưng không đưa vào 4 lý do cảnh báo cho seller.
            // =========================

            if (input.AccountAgeDays >= 90)
            {
                finalScore -= 5;
            }
            else if (input.AccountAgeDays >= 30)
            {
                finalScore -= 3;
            }

            if (input.TotalOrders >= 10)
            {
                finalScore -= 5;
            }
            else if (input.TotalOrders >= 5)
            {
                finalScore -= 3;
            }

            if (input.CompletedOrderCount >= 10)
            {
                finalScore -= 6;
            }
            else if (input.CompletedOrderCount >= 5)
            {
                finalScore -= 4;
            }

            if (input.TotalOrders >= 5 && input.CompletionRate >= 0.8f)
            {
                finalScore -= 6;
            }

            if (input.TotalOrders >= 5 && input.CancelRate <= 0.15f)
            {
                finalScore -= 5;
            }

            if (input.IsVip == 1 &&
                input.TotalOrders >= 5 &&
                input.CompletionRate >= 0.8f &&
                input.CancelRate <= 0.15f)
            {
                finalScore -= 10;
            }

            // =========================
            // 2. Tài khoản mới
            // =========================

            if (input.AccountAgeDays <= 3)
            {
                finalScore += 8;
                AddReason(
                    "Tuổi tài khoản",
                    8,
                    "Tài khoản khách hàng còn rất mới nên độ tin cậy lịch sử chưa cao."
                );
            }
            else if (input.AccountAgeDays <= 7)
            {
                finalScore += 5;
                AddReason(
                    "Tuổi tài khoản",
                    5,
                    "Tài khoản khách hàng mới dưới 7 ngày."
                );
            }
            else if (input.AccountAgeDays <= 15)
            {
                finalScore += 3;
                AddReason(
                    "Tuổi tài khoản",
                    3,
                    "Tài khoản khách hàng còn khá mới."
                );
            }

            // =========================
            // 3. Giá trị đơn hàng
            // =========================

            if (input.CurrentOrderValue >= 5000000)
            {
                finalScore += 25;
                AddReason(
                    "Giá trị đơn hàng",
                    25,
                    "Đơn hàng có giá trị rất cao so với mức thông thường."
                );
            }
            else if (input.CurrentOrderValue >= 3000000)
            {
                finalScore += 15;
                AddReason(
                    "Giá trị đơn hàng",
                    15,
                    "Đơn hàng có giá trị cao."
                );
            }
            else if (input.CurrentOrderValue >= 2000000)
            {
                finalScore += 8;
                AddReason(
                    "Giá trị đơn hàng",
                    8,
                    "Đơn hàng có giá trị tương đối cao."
                );
            }

            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 6 &&
                input.CurrentOrderValue >= 4000000)
            {
                finalScore += 80;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    80,
                    "Giá trị đơn hiện tại tăng đột biến rất mạnh so với trung bình các đơn trước của khách."
                );
            }
            else if (input.TotalOrders >= 3 &&
                     input.AvgOrderValue > 0 &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                     input.CurrentOrderValue >= 2500000)
            {
                finalScore += 45;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    45,
                    "Giá trị đơn hiện tại cao bất thường so với lịch sử mua hàng của khách."
                );
            }
            else if (input.TotalOrders >= 3 &&
                     input.AvgOrderValue > 0 &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 3 &&
                     input.CurrentOrderValue >= 1500000)
            {
                finalScore += 25;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    25,
                    "Giá trị đơn hiện tại cao hơn nhiều so với trung bình các đơn trước."
                );
            }

            // =========================
            // 4. Số lượng và số loại sản phẩm
            // =========================

            if (input.TotalQuantity >= 20)
            {
                finalScore += 25;
                AddReason(
                    "Số lượng sản phẩm",
                    25,
                    "Đơn hàng có tổng số lượng sản phẩm rất lớn."
                );
            }
            else if (input.TotalQuantity >= 12)
            {
                finalScore += 15;
                AddReason(
                    "Số lượng sản phẩm",
                    15,
                    "Đơn hàng có tổng số lượng sản phẩm lớn."
                );
            }
            else if (input.TotalQuantity >= 8)
            {
                finalScore += 8;
                AddReason(
                    "Số lượng sản phẩm",
                    8,
                    "Đơn hàng có số lượng sản phẩm hơi cao."
                );
            }

            if (input.ItemCount >= 6)
            {
                finalScore += 12;
                AddReason(
                    "Số loại sản phẩm",
                    12,
                    "Đơn hàng có nhiều loại sản phẩm khác nhau."
                );
            }
            else if (input.ItemCount >= 4)
            {
                finalScore += 8;
                AddReason(
                    "Số loại sản phẩm",
                    8,
                    "Đơn hàng có số loại sản phẩm hơi nhiều."
                );
            }

            // =========================
            // 5. Tần suất đặt đơn
            // =========================

            if (input.OrdersLast24h >= 5)
            {
                finalScore += 25;
                AddReason(
                    "Tần suất đặt đơn",
                    25,
                    "Khách đặt rất nhiều đơn trong 24 giờ gần đây."
                );
            }
            else if (input.OrdersLast24h >= 3)
            {
                finalScore += 18;
                AddReason(
                    "Tần suất đặt đơn",
                    18,
                    "Khách đặt nhiều đơn trong 24 giờ gần đây."
                );
            }
            else if (input.OrdersLast24h == 2)
            {
                finalScore += 5;
                AddReason(
                    "Tần suất đặt đơn",
                    5,
                    "Khách đặt 2 đơn trong 24 giờ gần đây."
                );
            }

            if (input.OrdersLast7d >= 7)
            {
                finalScore += 12;
                AddReason(
                    "Tần suất đặt đơn",
                    12,
                    "Khách đặt nhiều đơn trong 7 ngày gần đây."
                );
            }
            else if (input.OrdersLast7d >= 5)
            {
                finalScore += 6;
                AddReason(
                    "Tần suất đặt đơn",
                    6,
                    "Khách đặt khá nhiều đơn trong 7 ngày gần đây."
                );
            }

            // =========================
            // 6. Hủy đơn tổng thể
            // =========================

            if (input.TotalOrders >= 5 && input.CancelRate >= 0.7f)
            {
                finalScore += 35;
                AddReason(
                    "Tỷ lệ hủy đơn",
                    35,
                    "Khách có tỷ lệ hủy đơn rất cao trong lịch sử mua hàng."
                );
            }
            else if (input.TotalOrders >= 5 && input.CancelRate >= 0.5f)
            {
                finalScore += 25;
                AddReason(
                    "Tỷ lệ hủy đơn",
                    25,
                    "Khách có tỷ lệ hủy đơn cao trong lịch sử mua hàng."
                );
            }
            else if (input.TotalOrders >= 4 && input.CancelRate >= 0.35f)
            {
                finalScore += 12;
                AddReason(
                    "Tỷ lệ hủy đơn",
                    12,
                    "Khách có tỷ lệ hủy đơn ở mức cần chú ý."
                );
            }
            else if (input.TotalOrders >= 4 && input.CancelRate >= 0.2f)
            {
                finalScore += 5;
                AddReason(
                    "Tỷ lệ hủy đơn",
                    5,
                    "Khách có tỷ lệ hủy đơn hơi cao so với mức an toàn."
                );
            }

            if (input.CancelledOrders >= 5)
            {
                finalScore += 10;
                AddReason(
                    "Số đơn đã hủy",
                    10,
                    "Khách có nhiều đơn đã hủy trong lịch sử mua hàng."
                );
            }
            else if (input.CancelledOrders >= 3)
            {
                finalScore += 5;
                AddReason(
                    "Số đơn đã hủy",
                    5,
                    "Khách có một số đơn đã hủy trong lịch sử mua hàng."
                );
            }

            // =========================
            // 7. COD count
            // =========================

            if (input.CodOrderCount >= 10 && input.CancelRate >= 0.3f)
            {
                finalScore += 10;
                AddReason(
                    "Lịch sử COD",
                    10,
                    "Khách có nhiều đơn COD và tỷ lệ hủy đáng chú ý."
                );
            }
            else if (input.CodOrderCount >= 5 && input.CancelRate >= 0.3f)
            {
                finalScore += 5;
                AddReason(
                    "Lịch sử COD",
                    5,
                    "Khách có lịch sử COD kèm tỷ lệ hủy cần chú ý."
                );
            }

            // =========================
            // 8. Tổ hợp cộng điểm
            // =========================

            if (input.AccountAgeDays <= 7 &&
                input.CurrentOrderValue >= 3000000 &&
                input.TotalQuantity >= 8)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Tài khoản mới, đơn hàng giá trị cao và số lượng sản phẩm khá lớn xuất hiện cùng lúc."
                );
            }

            if (input.AccountAgeDays <= 7 &&
                input.CurrentOrderValue >= 5000000)
            {
                finalScore += 15;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    15,
                    "Tài khoản mới đi kèm đơn hàng có giá trị rất cao."
                );
            }

            if (input.AccountAgeDays <= 7 &&
                input.TotalQuantity >= 20 &&
                input.ItemCount >= 6)
            {
                finalScore += 20;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    20,
                    "Tài khoản mới, số lượng sản phẩm rất lớn và nhiều loại sản phẩm xuất hiện cùng lúc."
                );
            }

            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 6 &&
                input.CurrentOrderValue >= 4000000 &&
                input.TotalQuantity >= 8 &&
                input.ItemCount >= 4)
            {
                finalScore += 20;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    20,
                    "Đơn hàng vừa tăng đột biến so với lịch sử, vừa có giá trị cao và nhiều sản phẩm."
                );
            }

            if (input.AccountAgeDays <= 15 &&
                input.OrdersLast24h >= 3 &&
                input.CurrentOrderValue >= 1000000)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Tài khoản mới, đặt nhiều đơn trong 24 giờ và đơn hiện tại có giá trị đáng chú ý."
                );
            }

            if (input.TotalOrders >= 5 &&
                input.CancelRate >= 0.3f &&
                input.CurrentOrderValue >= 2000000)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Khách có tỷ lệ hủy đáng chú ý và đơn hiện tại có giá trị cao."
                );
            }

            if (input.TotalOrders >= 5 &&
                input.CompletionRate < 0.6f &&
                input.CurrentOrderValue >= 1500000)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Khách có tỷ lệ hoàn thành đơn thấp và đơn hiện tại có giá trị đáng chú ý."
                );
            }

            // =========================
            // 9. Giới hạn điểm cho khách tốt
            // =========================

            if (IsGoodCustomer(input) &&
                !IsStrongOrderValueSpike(input) &&
                finalScore > 60)
            {
                finalScore = 60;
            }

            // =========================
            // 10. Giới hạn các case chỉ nên ở mức Medium
            // =========================

            if (IsMediumOnlyCase(input) && finalScore > 70)
            {
                finalScore = 70;
            }

            if (IsExtremeOrderValueSpike(input) && finalScore < 85)
            {
                finalScore = 85;

                AddReason(
                    "So sánh với lịch sử mua hàng",
                    85,
                    "Đơn hàng tăng đột biến cực mạnh so với lịch sử mua hàng nên được xếp vào mức cảnh báo cao."
                );
            }

            // =========================
            // 11. Phân mức cuối cùng
            // =========================

            string riskLevel;

            if (finalScore >= 80)
            {
                riskLevel = "High";
            }
            else if (finalScore >= 20)
            {
                riskLevel = "Medium";
            }
            else
            {
                riskLevel = "Low";
            }

            var topReasons = reasonScores
                .Where(x => x.Impact > 0)
                .GroupBy(x => x.Field)
                .Select(g => g
                    .OrderByDescending(x => Math.Abs(x.Impact))
                    .First())
                .OrderByDescending(x => Math.Abs(x.Impact))
                .Take(4)
                .Select(x => $"{x.Field}: {x.Message}")
                .ToList();

            if (!topReasons.Any())
            {
                topReasons.Add("Không phát hiện dấu hiệu bất thường rõ ràng trong đơn hàng này.");
            }

            var warningMessage = BuildWarningMessage(riskLevel, reasonScores);

            return new OrderRiskDecision
            {
                RiskLevel = riskLevel,
                WarningMessage = warningMessage,
                Suggestion = GetSuggestion(riskLevel),
                Reasons = topReasons,
                FinalScore = finalScore
            };
        }
        private static string BuildWarningMessage(
    string riskLevel,
    List<(string Field, float Impact, string Message)> reasonScores)
        {
            if (riskLevel == "DataWarning")
            {
                return "Đơn hàng thiếu dữ liệu cần thiết nên hệ thống chưa thể đánh giá rủi ro chính xác.";
            }

            if (riskLevel == "Low")
            {
                return "Đơn hàng không có dấu hiệu rủi ro rõ ràng dựa trên dữ liệu hiện tại.";
            }

            var topFields = reasonScores
                .Where(x => x.Impact > 0)
                .GroupBy(x => x.Field)
                .Select(g => g
                    .OrderByDescending(x => Math.Abs(x.Impact))
                    .First())
                .OrderByDescending(x => Math.Abs(x.Impact))
                .Take(2)
                .Select(x => x.Field.ToLower())
                .ToList();

            if (!topFields.Any())
            {
                return "Đơn hàng có một số dấu hiệu cần được kiểm tra thêm trước khi xử lý.";
            }

            string fieldText;

            if (topFields.Count == 1)
            {
                fieldText = topFields[0];
            }
            else
            {
                fieldText = $"{topFields[0]} và {topFields[1]}";
            }

            if (riskLevel == "High")
            {
                return $"Đơn hàng có mức rủi ro cao do {fieldText} có dấu hiệu bất thường.";
            }

            if (riskLevel == "Medium")
            {
                return $"Đơn hàng có một số dấu hiệu cần chú ý liên quan đến {fieldText}.";
            }

            return "Đơn hàng không có dấu hiệu rủi ro rõ ràng.";
        }
        private static bool IsExtremeOrderValueSpike(OrderRiskTrainingData input)
        {
            return input.TotalOrders >= 5 &&
                   input.AvgOrderValue > 0 &&
                   input.CurrentOrderValue >= input.AvgOrderValue * 7 &&
                   input.CurrentOrderValue >= 5000000 &&
                   input.TotalQuantity >= 8 &&
                   input.ItemCount >= 4;
        }
        private static bool IsMediumOnlyCase(OrderRiskTrainingData input)
        {
            return input.CurrentOrderValue < 3000000 &&
                   input.TotalQuantity < 10 &&
                   input.ItemCount <= 5 &&
                   input.OrdersLast24h <= 1 &&
                   input.OrdersLast7d <= 2 &&
                   input.CancelRate < 0.35f &&
                   input.CompletionRate >= 0.7f;
        }
        
        private static bool IsGoodCustomer(OrderRiskTrainingData input)
        {
            return input.AccountAgeDays >= 30 &&
                   input.TotalOrders >= 5 &&
                   input.CompletedOrderCount >= 4 &&
                   input.CompletionRate >= 0.8f &&
                   input.CancelRate <= 0.15f &&
                   input.CurrentOrderValue <= 3000000 &&
                   input.TotalQuantity <= 10 &&
                   input.ItemCount <= 5;
        }
        private static bool IsStrongOrderValueSpike(OrderRiskTrainingData input)
        {
            return input.TotalOrders >= 3 &&
                   input.AvgOrderValue > 0 &&
                   input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                   input.CurrentOrderValue >= 2500000;
        }
        private static float NormalizeAiScore(float aiScore)
        {
            if (aiScore >= 80)
            {
                return 50;
            }

            if (aiScore >= 50)
            {
                return 35;
            }

            if (aiScore >= 0)
            {
                return 20;
            }

            if (aiScore <= -50)
            {
                return -30;
            }

            return -10;
        }
    }
}