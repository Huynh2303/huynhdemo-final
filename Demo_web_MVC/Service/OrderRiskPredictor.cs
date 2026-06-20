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
                    Suggestion = GetSuggestion("DataWarning"),
                    FinalScore = 0,
                    Reasons = new List<string>
            {
                "Đơn hàng thiếu dữ liệu sản phẩm, tổng tiền hoặc số lượng."
            }
                };
            }

            var reasons = new List<string>();

            // Không dùng prediction.IsRisk.
            // Chỉ lấy Score của AI làm điểm nền.
            float aiBaseScore = NormalizeAiScore(prediction.Score);
            float finalScore = aiBaseScore;

            reasons.Add($"Điểm AI ban đầu: {prediction.Score:F2}, quy đổi thành {aiBaseScore:F2} điểm nền.");

            // =========================
            // 1. Điểm bảo vệ khách tốt
            // =========================

            if (input.AccountAgeDays >= 90)
            {
                finalScore -= 5;
                reasons.Add("Tài khoản đã hoạt động trên 90 ngày: -5 điểm.");
            }
            else if (input.AccountAgeDays >= 30)
            {
                finalScore -= 3;
                reasons.Add("Tài khoản đã hoạt động trên 30 ngày: -3 điểm.");
            }

            if (input.TotalOrders >= 10)
            {
                finalScore -= 5;
                reasons.Add("Khách có lịch sử mua hàng tương đối nhiều: -5 điểm.");
            }
            else if (input.TotalOrders >= 5)
            {
                finalScore -= 3;
                reasons.Add("Khách đã có từ 5 đơn trở lên: -3 điểm.");
            }

            if (input.CompletedOrderCount >= 10)
            {
                finalScore -= 6;
                reasons.Add("Khách có lịch sử hoàn thành đơn tốt: -6 điểm.");
            }
            else if (input.CompletedOrderCount >= 5)
            {
                finalScore -= 4;
                reasons.Add("Khách có nhiều đơn hoàn thành: -4 điểm.");
            }

            if (input.TotalOrders >= 5 && input.CompletionRate >= 0.8f)
            {
                finalScore -= 6;
                reasons.Add("Tỷ lệ hoàn thành đơn tốt: -6 điểm.");
            }

            if (input.TotalOrders >= 5 && input.CancelRate <= 0.15f)
            {
                finalScore -= 5;
                reasons.Add("Tỷ lệ hủy đơn thấp: -5 điểm.");
            }

            if (input.IsVip == 1 &&
                input.TotalOrders >= 5 &&
                input.CompletionRate >= 0.8f &&
                input.CancelRate <= 0.15f)
            {
                finalScore -= 10;
                reasons.Add("Khách VIP có lịch sử tốt: -10 điểm.");
            }

            // =========================
            // 2. Điểm rủi ro tài khoản mới
            // =========================

            if (input.AccountAgeDays <= 3)
            {
                finalScore += 8;
                reasons.Add("Tài khoản rất mới: +8 điểm.");
            }
            else if (input.AccountAgeDays <= 7)
            {
                finalScore += 5;
                reasons.Add("Tài khoản mới dưới 7 ngày: +5 điểm.");
            }
            else if (input.AccountAgeDays <= 15)
            {
                finalScore += 3;
                reasons.Add("Tài khoản còn khá mới: +3 điểm.");
            }

            // =========================
            // 3. Giá trị đơn hàng
            // =========================

            if (input.CurrentOrderValue >= 5000000)
            {
                finalScore += 25;
                reasons.Add("Đơn hàng có giá trị rất cao: +25 điểm.");
            }
            else if (input.CurrentOrderValue >= 3000000)
            {
                finalScore += 15;
                reasons.Add("Đơn hàng có giá trị cao: +15 điểm.");
            }
            else if (input.CurrentOrderValue >= 2000000)
            {
                finalScore += 8;
                reasons.Add("Đơn hàng có giá trị tương đối cao: +8 điểm.");
            }

            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 6 &&
                input.CurrentOrderValue >= 4000000)
                        {
                finalScore += 80;
                reasons.Add("Giá trị đơn hiện tại tăng đột biến rất mạnh so với trung bình lịch sử: +80 điểm.");
            }
            else if (input.TotalOrders >= 3 &&
                     input.AvgOrderValue > 0 &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                     input.CurrentOrderValue >= 2500000)
            {
                finalScore += 45;
                reasons.Add("Giá trị đơn hiện tại cao bất thường so với trung bình lịch sử: +45 điểm.");
            }
            else if (input.TotalOrders >= 3 &&
                     input.AvgOrderValue > 0 &&
                     input.CurrentOrderValue >= input.AvgOrderValue * 3 &&
                     input.CurrentOrderValue >= 1500000)
            {
                finalScore += 25;
                reasons.Add("Giá trị đơn hiện tại cao hơn nhiều so với trung bình lịch sử: +25 điểm.");
            }
            // =========================
            // 4. Số lượng và số loại sản phẩm
            // =========================

            if (input.TotalQuantity >= 20)
            {
                finalScore += 25;
                reasons.Add("Số lượng sản phẩm rất lớn: +25 điểm.");
            }
            else if (input.TotalQuantity >= 12)
            {
                finalScore += 15;
                reasons.Add("Số lượng sản phẩm lớn: +15 điểm.");
            }
            else if (input.TotalQuantity >= 8)
            {
                finalScore += 8;
                reasons.Add("Số lượng sản phẩm hơi cao: +8 điểm.");
            }

            if (input.ItemCount >= 6)
            {
                finalScore += 12;
                reasons.Add("Đơn có nhiều loại sản phẩm khác nhau: +12 điểm.");
            }
            else if (input.ItemCount >= 4)
            {
                finalScore += 8;
                reasons.Add("Đơn có số loại sản phẩm hơi nhiều: +8 điểm.");
            }

            // =========================
            // 5. Tần suất đặt đơn
            // =========================

            if (input.OrdersLast24h >= 5)
            {
                finalScore += 25;
                reasons.Add("Khách đặt rất nhiều đơn trong 24 giờ gần đây: +25 điểm.");
            }
            else if (input.OrdersLast24h >= 3)
            {
                finalScore += 18;
                reasons.Add("Khách đặt nhiều đơn trong 24 giờ gần đây: +18 điểm.");
            }
            else if (input.OrdersLast24h == 2)
            {
                finalScore += 5;
                reasons.Add("Khách đặt 2 đơn trong 24 giờ gần đây: +5 điểm.");
            }

            if (input.OrdersLast7d >= 7)
            {
                finalScore += 12;
                reasons.Add("Khách đặt nhiều đơn trong 7 ngày gần đây: +12 điểm.");
            }
            else if (input.OrdersLast7d >= 5)
            {
                finalScore += 6;
                reasons.Add("Khách đặt khá nhiều đơn trong 7 ngày gần đây: +6 điểm.");
            }

            // =========================
            // 6. Hủy đơn tổng thể
            // =========================

            if (input.TotalOrders >= 5 && input.CancelRate >= 0.7f)
            {
                finalScore += 35;
                reasons.Add("Tỷ lệ hủy đơn tổng thể rất cao: +30 điểm.");
            }
            else if (input.TotalOrders >= 5 && input.CancelRate >= 0.5f)
            {
                finalScore += 25;
                reasons.Add("Tỷ lệ hủy đơn tổng thể cao: +20 điểm.");
            }
            else if (input.TotalOrders >= 4 && input.CancelRate >= 0.35f)
            {
                finalScore += 12;
                reasons.Add("Tỷ lệ hủy đơn ở mức cần chú ý: +10 điểm.");
            }
            else if (input.TotalOrders >= 4 && input.CancelRate >= 0.2f)
            {
                finalScore += 5;
                reasons.Add("Tỷ lệ hủy đơn hơi cao nhưng chưa nghiêm trọng: +4 điểm.");
            }

            if (input.CancelledOrders >= 5)
            {
                finalScore += 10;
                reasons.Add("Khách có nhiều đơn đã hủy: +10 điểm.");
            }
            else if (input.CancelledOrders >= 3)
            {
                finalScore += 5;
                reasons.Add("Khách có một số đơn đã hủy: +5 điểm.");
            }

            // =========================
            // 7. COD count
            // Không dùng IsCod vì bạn đã loại khỏi train.
            // Chỉ dùng CodOrderCount vì vẫn còn trong Features.
            // =========================

            if (input.CodOrderCount >= 10 && input.CancelRate >= 0.3f)
            {
                finalScore += 10;
                reasons.Add("Khách có nhiều đơn COD và tỷ lệ hủy đáng chú ý: +10 điểm.");
            }
            else if (input.CodOrderCount >= 5 && input.CancelRate >= 0.3f)
            {
                finalScore += 5;
                reasons.Add("Khách có lịch sử COD kèm tỷ lệ hủy cần chú ý: +5 điểm.");
            }

            // =========================
            // 8. Tổ hợp cộng điểm nhẹ
            // Không return High ngay.
            // =========================

            if (input.AccountAgeDays <= 7 &&
                input.CurrentOrderValue >= 3000000 &&
                input.TotalQuantity >= 8)
            {
                finalScore += 10;
                reasons.Add("Tổ hợp tài khoản mới, đơn cao và số lượng khá lớn: +10 điểm.");
            }
            if (input.AccountAgeDays <= 7 &&
                input.CurrentOrderValue >= 5000000)
            {
                finalScore += 15;
                reasons.Add("Tổ hợp tài khoản mới và đơn hàng giá trị rất cao: +15 điểm.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.TotalQuantity >= 20 &&
                input.ItemCount >= 6)
            {
                finalScore += 20;
                reasons.Add("Tổ hợp tài khoản mới, số lượng rất lớn và nhiều loại sản phẩm: +20 điểm.");
            }

            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 6 &&
                input.CurrentOrderValue >= 4000000 &&
                input.TotalQuantity >= 8 &&
                input.ItemCount >= 4)
            {
                finalScore += 20;
                reasons.Add("Tổ hợp đơn tăng đột biến, giá trị cao và nhiều sản phẩm: +20 điểm.");
            }
            if (input.AccountAgeDays <= 15 &&
                input.OrdersLast24h >= 3 &&
                input.CurrentOrderValue >= 1000000)
            {
                finalScore += 10;
                reasons.Add("Tổ hợp tài khoản mới, đặt nhiều đơn trong 24 giờ và giá trị đơn đáng chú ý: +10 điểm.");
            }

            if (input.TotalOrders >= 5 &&
                input.CancelRate >= 0.3f &&
                input.CurrentOrderValue >= 2000000)
            {
                finalScore += 10;
                reasons.Add("Tổ hợp tỷ lệ hủy đáng chú ý và đơn hiện tại có giá trị cao: +10 điểm.");
            }

            if (input.TotalOrders >= 5 &&
                input.CompletionRate < 0.6f &&
                input.CurrentOrderValue >= 1500000)
            {
                finalScore += 10;
                reasons.Add("Tổ hợp tỷ lệ hoàn thành thấp và đơn hiện tại có giá trị đáng chú ý: +10 điểm.");
            }

            // =========================
            // 9. Giới hạn điểm cho khách tốt
            // Tránh khách tốt bị đẩy lên High chỉ vì vài tín hiệu nhẹ.
            // =========================

            if (IsGoodCustomer(input) &&
    !IsStrongOrderValueSpike(input) &&
    finalScore > 60)
            {
                finalScore = 60;
                reasons.Add("Khách có lịch sử tốt nên giới hạn điểm rủi ro tối đa ở mức Medium.");
            }

            // =========================
            // 10. Giới hạn các case chỉ nên ở mức Medium
            // =========================

            if (IsMediumOnlyCase(input) && finalScore > 70)
            {
                finalScore = 70;
                reasons.Add("Dữ liệu chỉ ở mức cần xem xét, chưa đủ dấu hiệu để xếp High nên giới hạn ở Medium.");
            }

            if (IsExtremeOrderValueSpike(input) && finalScore < 85)
            {
                finalScore = 85;
                reasons.Add("Đơn hàng tăng đột biến cực mạnh so với lịch sử nên tối thiểu xếp mức High.");
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

            reasons.Add($"Điểm rủi ro cuối cùng: {finalScore:F2}.");

            return new OrderRiskDecision
            {
                RiskLevel = riskLevel,
                Suggestion = GetSuggestion(riskLevel),
                Reasons = reasons,
                FinalScore = finalScore
            };
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
        //private OrderRiskDecision LowDecision()
        //{
        //    return new OrderRiskDecision
        //    {
        //        RiskLevel = "Low",
        //        Suggestion = GetSuggestion("Low"),
        //        Reasons = new List<string>
        //        {
        //            "Không phát hiện dấu hiệu rủi ro đáng chú ý."
        //        }
        //    };
        //}

        //private static bool IsTrustedCustomer(OrderRiskTrainingData input)
        //{
        //    return input.AccountAgeDays >= 30 &&
        //           input.TotalOrders >= 3 &&
        //           input.CancelRate == 0 &&
        //           input.CancelRateLast24h == 0 &&
        //           input.CancelRateLast7d == 0 &&
        //           input.PhoneUsedCount <= 1 &&
        //           input.AddressUsedCount <= 1 &&
        //           input.OrdersLast24h <= 1 &&
        //           input.StatusChangeCount <= 2 &&
        //           input.CurrentOrderValue <= 3000000 &&
        //           input.TotalQuantity <= 10 &&
        //           input.ItemCount <= 5;
        //}

        //private static bool IsClearlyLowRiskOrder(OrderRiskTrainingData input)
        //{
        //    return input.CurrentOrderValue < 1500000 &&
        //           input.TotalQuantity <= 5 &&
        //           input.ItemCount <= 3 &&
        //           input.OrdersLast24h <= 1 &&
        //           input.OrdersLast7d <= 2 &&
        //           input.TotalOrders <= 2 &&
        //           input.CancelRate == 0 &&
        //           input.CancelRateLast24h == 0 &&
        //           input.CancelRateLast7d == 0 &&
        //           input.PhoneUsedCount <= 1 &&
        //           input.AddressUsedCount <= 1 &&
        //           input.StatusChangeCount <= 2;
        //}

        //private static bool HasEnoughOrderHistory(OrderRiskTrainingData input)
        //{
        //    return input.TotalOrders >= 4;
        //}

        //private static bool ShouldAcceptAiWarning(OrderRiskTrainingData input)
        //{
        //    if (IsClearlyLowRiskOrder(input) || IsTrustedCustomer(input))
        //    {
        //        return false;
        //    }

        //    return input.CurrentOrderValue >= 2000000 ||
        //           input.TotalQuantity >= 10 ||
        //           input.ItemCount >= 5 ||
        //           input.OrdersLast24h >= 2 ||
        //           input.OrdersLast7d >= 4 ||
        //           input.TotalOrders >= 4 ||
        //           input.CancelRate >= 0.35f ||
        //           input.CancelRateLast24h > 0 ||
        //           input.CancelRateLast7d >= 0.4f ||
        //           input.PhoneUsedCount >= 2 ||
        //           input.AddressUsedCount >= 2 ||
        //           input.StatusChangeCount >= 5;
        //}
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