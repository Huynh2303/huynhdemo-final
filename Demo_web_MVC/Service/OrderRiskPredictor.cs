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

        public OrderRiskDecision GetRiskDecision(
            OrderRiskTrainingData input,
            OrderRiskPrediction prediction)
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

            // Chỉ lấy Score của AI làm điểm nền.
            // Không dùng trực tiếp prediction.IsRisk để quyết định cuối cùng.
            float aiBaseScore = NormalizeAiScore(prediction.Score);
            float finalScore = aiBaseScore;

            var orderValueThresholds = GetOrderValueThresholds(input);

            // =========================
            // 1. Điểm bảo vệ khách tốt
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

            if (IsTrustedVipCustomer(input))
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
            // Ngưỡng tiền thay đổi theo độ uy tín khách.
            // VIP tốt có ngưỡng cao hơn khách thường.
            // =========================

            if (input.CurrentOrderValue >= orderValueThresholds.VeryHighValue)
            {
                finalScore += 20;
                AddReason(
                    "Giá trị đơn hàng",
                    20,
                    "Đơn hàng có giá trị rất cao, nên kiểm tra kỹ trước khi xử lý."
                );
            }
            else if (input.CurrentOrderValue >= orderValueThresholds.HighValue)
            {
                finalScore += 12;
                AddReason(
                    "Giá trị đơn hàng",
                    12,
                    "Đơn hàng có giá trị cao."
                );
            }
            else if (input.CurrentOrderValue >= orderValueThresholds.MediumValue)
            {
                finalScore += 8;
                AddReason(
                    "Giá trị đơn hàng",
                    8,
                    "Đơn hàng có giá trị tương đối cao."
                );
            }

            // =========================
            // 4. So sánh với lịch sử mua hàng
            // Không so spike quá mạnh nếu AvgOrderValue quá thấp.
            // Với VIP tốt, ngưỡng spike cao hơn.
            // =========================

            var minAvgValueForSpike = IsTrustedVipCustomer(input)
                ? 5000000
                : 3000000;

            var spikeLevel1Value = IsTrustedVipCustomer(input)
                ? 25000000
                : 15000000;

            var spikeLevel2Value = IsTrustedVipCustomer(input)
                ? 35000000
                : 20000000;

            var spikeLevel3Value = IsTrustedVipCustomer(input)
                ? 50000000
                : 30000000;

            if (input.TotalOrders >= 5 &&
                input.AvgOrderValue >= minAvgValueForSpike &&
                input.CurrentOrderValue >= input.AvgOrderValue * 5 &&
                input.CurrentOrderValue >= spikeLevel3Value)
            {
                finalScore += 35;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    35,
                    "Giá trị đơn hiện tại cao bất thường so với lịch sử mua hàng trước đó."
                );
            }
            else if (input.TotalOrders >= 5 &&
                     input.AvgOrderValue >= minAvgValueForSpike &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                     input.CurrentOrderValue >= spikeLevel2Value)
            {
                finalScore += 25;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    25,
                    "Giá trị đơn hiện tại cao hơn nhiều so với lịch sử mua hàng của khách."
                );
            }
            else if (input.TotalOrders >= 5 &&
                     input.AvgOrderValue >= minAvgValueForSpike &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 3 &&
                     input.CurrentOrderValue >= spikeLevel1Value)
            {
                finalScore += 15;
                AddReason(
                    "So sánh với lịch sử mua hàng",
                    15,
                    "Giá trị đơn hiện tại cao hơn lịch sử mua hàng thông thường của khách."
                );
            }

            // =========================
            // 5. Số lượng và số loại sản phẩm
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
            // 6. Tần suất đặt đơn
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
            // 7. Hủy đơn tổng thể
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
            // 8. Lịch sử COD
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
            // 9. Tổ hợp cộng điểm
            // Không kéo High chỉ vì giá trị đơn cao.
            // =========================

            if (input.AccountAgeDays <= 7 &&
                input.CurrentOrderValue >= orderValueThresholds.MediumValue &&
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
                input.CurrentOrderValue >= orderValueThresholds.HighValue)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Tài khoản mới đi kèm đơn hàng có giá trị cao."
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

            if (input.TotalOrders >= 5 &&
                input.AvgOrderValue >= minAvgValueForSpike &&
                input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                input.CurrentOrderValue >= spikeLevel2Value &&
                input.TotalQuantity >= 8 &&
                input.ItemCount >= 4)
            {
                finalScore += 15;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    15,
                    "Đơn hàng vừa cao hơn lịch sử, vừa có số lượng và số loại sản phẩm đáng chú ý."
                );
            }

            if (input.AccountAgeDays <= 15 &&
                input.OrdersLast24h >= 3 &&
                input.CurrentOrderValue >= orderValueThresholds.MediumValue)
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
                input.CurrentOrderValue >= orderValueThresholds.MediumValue)
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
                input.CurrentOrderValue >= orderValueThresholds.MediumValue)
            {
                finalScore += 10;
                AddReason(
                    "Tổ hợp dấu hiệu rủi ro",
                    10,
                    "Khách có tỷ lệ hoàn thành đơn thấp và đơn hiện tại có giá trị đáng chú ý."
                );
            }

            // =========================
            // 10. Giới hạn điểm cho khách tốt
            // Khách tốt/VIP tốt không bị đẩy High chỉ vì đơn giá trị cao vừa phải.
            // =========================

            if (IsGoodCustomer(input) &&
                !IsStrongOrderValueSpike(input) &&
                !IsCriticalHighRiskCase(input) &&
                !IsHighRiskByBadHistory(input) &&
                finalScore > 60)
            {
                finalScore = 60;
            }

            // =========================
            // 11. Chặn các case chỉ nên Medium
            // =========================

            if (IsMediumOnlyCase(input) &&
                !IsCriticalHighRiskCase(input) &&
                !IsHighRiskByBadHistory(input) &&
                !IsExtremeOrderValueSpike(input) &&
                finalScore > 70)
            {
                finalScore = 70;
            }

            // =========================
            // 12. Sàn High cho case thật sự nghiêm trọng
            // =========================

            if (IsCriticalHighRiskCase(input) && finalScore < 85)
            {
                finalScore = 85;

                AddReason(
                    "Tổ hợp rủi ro nghiêm trọng",
                    85,
                    "Đơn hàng có nhiều dấu hiệu rủi ro nghiêm trọng xuất hiện cùng lúc."
                );
            }

            if (IsHighRiskByBadHistory(input) && finalScore < 85)
            {
                finalScore = 85;

                AddReason(
                    "Lịch sử mua hàng",
                    85,
                    "Đơn hàng có giá trị cao đi kèm lịch sử mua hàng yếu nên được xếp vào mức cảnh báo cao."
                );
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
            // 13. Bảo vệ VIP tốt mua đơn dưới 10 triệu
            // =========================

            if (IsTrustedCustomerLowValueOrder(input) && finalScore < 80)
            {
                finalScore = 15;

                reasonScores.RemoveAll(x =>
                    x.Field == "Giá trị đơn hàng" ||
                    x.Field == "Tuổi tài khoản" ||
                    x.Field == "Tổ hợp dấu hiệu rủi ro"
                );
            }

            // =========================
            // 14. Sàn Medium cho đơn giá trị đáng chú ý
            // =========================

            if (IsSignificantOrderValueCase(input) && finalScore < 35)
            {
                finalScore = 35;

                AddReason(
                    "Giá trị đơn hàng",
                    35,
                    "Đơn hàng có giá trị đáng chú ý nên cần được kiểm tra trước khi xử lý."
                );
            }

            // =========================
            // 15. Giảm nhạy cho đơn đầu bình thường
            // =========================

            if (IsNewCustomerNormalOrder(input) && finalScore < 40)
            {
                finalScore = 15;

                reasonScores.RemoveAll(x => x.Field == "Tuổi tài khoản");
            }

            // =========================
            // 16. Phân mức cuối cùng
            // =========================

            string riskLevel;

            if (finalScore >= 80)
            {
                riskLevel = "High";
            }
            else if (finalScore >= 30)
            {
                riskLevel = "Medium";
            }
            else
            {
                riskLevel = "Low";
            }

            var topReasons = BuildTopReasons(riskLevel, reasonScores);

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

        private static List<string> BuildTopReasons(
            string riskLevel,
            List<(string Field, float Impact, string Message)> reasonScores)
        {
            if (riskLevel == "Low")
            {
                return new List<string>
                {
                    "Không phát hiện dấu hiệu bất thường rõ ràng trong đơn hàng này."
                };
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
                topReasons.Add("Đơn hàng có một số dấu hiệu cần kiểm tra thêm trước khi xử lý.");
            }

            return topReasons;
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

            var fieldText = topFields.Count == 1
                ? topFields[0]
                : $"{topFields[0]} và {topFields[1]}";

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

        private static bool IsTrustedVipCustomer(OrderRiskTrainingData input)
        {
            return input.IsVip == 1 &&
                   input.TotalOrders >= 5 &&
                   input.CompletedOrderCount >= 5 &&
                   input.CompletionRate >= 0.8f &&
                   input.CancelRate <= 0.15f;
        }

        private static bool IsGoodHistoryCustomer(OrderRiskTrainingData input)
        {
            return input.AccountAgeDays >= 30 &&
                   input.TotalOrders >= 5 &&
                   input.CompletedOrderCount >= 4 &&
                   input.CompletionRate >= 0.8f &&
                   input.CancelRate <= 0.15f;
        }

        private static (float MediumValue, float HighValue, float VeryHighValue)
            GetOrderValueThresholds(OrderRiskTrainingData input)
        {
            if (IsTrustedVipCustomer(input))
            {
                // VIP tốt:
                // Dưới 10 triệu có thể Low nếu không có dấu hiệu xấu.
                // Từ 10 triệu bắt đầu đáng chú ý.
                return (10000000, 30000000, 50000000);
            }

            if (IsGoodHistoryCustomer(input))
            {
                // Khách có lịch sử tốt nhưng chưa đủ VIP.
                return (12000000, 25000000, 40000000);
            }

            // Khách thường hoặc lịch sử chưa đẹp.
            return (8000000, 15000000, 30000000);
        }

        private static bool IsGoodCustomer(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            return IsGoodHistoryCustomer(input) &&
                   input.CurrentOrderValue <= thresholds.HighValue &&
                   input.TotalQuantity <= 10 &&
                   input.ItemCount <= 5;
        }

        private static bool IsTrustedCustomerLowValueOrder(OrderRiskTrainingData input)
        {
            return IsTrustedVipCustomer(input) &&
                   input.CurrentOrderValue < 10000000 &&
                   input.TotalQuantity <= 3 &&
                   input.ItemCount <= 2 &&
                   input.OrdersLast24h <= 1 &&
                   input.OrdersLast7d <= 3 &&
                   input.CancelledOrders == 0 &&
                   input.CancelRate <= 0.15f &&
                   input.CompletionRate >= 0.8f;
        }

        private static bool IsNewCustomerNormalOrder(OrderRiskTrainingData input)
        {
            return input.AccountAgeDays <= 7 &&
                   input.TotalOrders == 0 &&
                   input.CancelledOrders == 0 &&
                   input.OrdersLast24h == 0 &&
                   input.OrdersLast7d == 0 &&
                   input.CurrentOrderValue < 2000000 &&
                   input.TotalQuantity <= 3 &&
                   input.ItemCount <= 2;
        }

        private static bool IsMediumOnlyCase(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            return input.CurrentOrderValue < thresholds.HighValue &&
                   input.TotalQuantity < 10 &&
                   input.ItemCount <= 5 &&
                   input.OrdersLast24h <= 1 &&
                   input.OrdersLast7d <= 2 &&
                   input.CancelRate < 0.35f &&
                   input.CompletionRate >= 0.7f;
        }

        private static bool IsSignificantOrderValueCase(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            return input.CurrentOrderValue >= thresholds.MediumValue &&
                   input.TotalQuantity <= 10 &&
                   input.ItemCount <= 5;
        }

        private static bool IsStrongOrderValueSpike(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            var minAvgValueForSpike = IsTrustedVipCustomer(input)
                ? 5000000
                : 3000000;

            return input.TotalOrders >= 5 &&
                   input.AvgOrderValue >= minAvgValueForSpike &&
                   input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                   input.CurrentOrderValue >= thresholds.HighValue;
        }

        private static bool IsExtremeOrderValueSpike(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            var minAvgValueForSpike = IsTrustedVipCustomer(input)
                ? 5000000
                : 3000000;

            return input.TotalOrders >= 5 &&
                   input.AvgOrderValue >= minAvgValueForSpike &&
                   input.CurrentOrderValue >= input.AvgOrderValue * 5 &&
                   input.CurrentOrderValue >= thresholds.VeryHighValue &&
                   input.TotalQuantity >= 8 &&
                   input.ItemCount >= 4;
        }

        private static bool IsHighRiskByBadHistory(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            // VIP tốt không bị kéo High bởi rule lịch sử xấu.
            if (IsTrustedVipCustomer(input))
            {
                return false;
            }

            var highValueWithVeryWeakCompletion =
                input.TotalOrders >= 10 &&
                input.CurrentOrderValue >= thresholds.HighValue &&
                input.CompletionRate < 0.3f &&
                input.CompletedOrderCount <= 2;

            var highValueWithManyCancelledOrders =
                input.TotalOrders >= 10 &&
                input.CurrentOrderValue >= thresholds.HighValue &&
                input.CancelledOrders >= 5 &&
                input.CancelRate >= 0.2f;

            var veryHighValueWithWeakHistory =
                input.TotalOrders >= 5 &&
                input.CurrentOrderValue >= thresholds.VeryHighValue &&
                (
                    input.CompletionRate < 0.5f ||
                    input.CancelledOrders >= 5 ||
                    input.CancelRate >= 0.2f
                );

            return highValueWithVeryWeakCompletion ||
                   highValueWithManyCancelledOrders ||
                   veryHighValueWithWeakHistory;
        }

        private static bool IsCriticalHighRiskCase(OrderRiskTrainingData input)
        {
            var thresholds = GetOrderValueThresholds(input);

            var veryHighValueWithVeryBadHistory =
                input.CurrentOrderValue >= thresholds.VeryHighValue &&
                input.TotalOrders >= 5 &&
                input.CancelRate >= 0.5f &&
                input.CancelledOrders >= 5;

            var veryHighValueWithBurstOrders =
                input.CurrentOrderValue >= thresholds.VeryHighValue &&
                input.OrdersLast24h >= 3;

            var veryHighValueWithHugeBasket =
                input.CurrentOrderValue >= thresholds.VeryHighValue &&
                input.TotalQuantity >= 20 &&
                input.ItemCount >= 6;

            var severeCancellationPattern =
                input.TotalOrders >= 5 &&
                input.CancelRate >= 0.7f &&
                input.CancelledOrders >= 5 &&
                input.CurrentOrderValue >= thresholds.HighValue;

            return veryHighValueWithVeryBadHistory ||
                   veryHighValueWithBurstOrders ||
                   veryHighValueWithHugeBasket ||
                   severeCancellationPattern;
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