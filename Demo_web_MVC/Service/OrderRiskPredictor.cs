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
                    Reasons = new List<string>
                    {
                        "Đơn hàng thiếu dữ liệu sản phẩm, tổng tiền hoặc số lượng."
                    }
                };
            }

            // Khách hàng cũ, lịch sử tốt thì ưu tiên Low.
            // Không để AI kéo nhóm này lên Medium.
            if (IsTrustedCustomer(input))
            {
                return LowDecision();
            }

            // Đơn hàng rõ ràng bình thường thì trả Low sớm.
            // Ví dụ: tài khoản mới đặt đơn nhỏ, ít sản phẩm, không trùng SĐT/địa chỉ, không có lịch sử hủy.
            if (IsClearlyLowRiskOrder(input))
            {
                return LowDecision();
            }

            var highReasons = new List<string>();

            

            if (input.OrdersLast24h >= 5)
            {
                highReasons.Add($"Khách đã đặt {input.OrdersLast24h} đơn trong 24 giờ gần đây.");
            }

            if (input.OrdersLast24h >= 3 &&
                input.CancelRateLast24h >= 0.7f)
            {
                highReasons.Add("Khách đặt nhiều đơn trong 24 giờ và tỷ lệ hủy gần đây rất cao.");
            }

            if (input.TotalOrders >= 5 &&
                input.CancelRate >= 0.7f)
            {
                highReasons.Add($"Tỷ lệ hủy đơn tổng thể rất cao: {input.CancelRate:P0}.");
            }

            if (input.OrdersLast7d >= 7 &&
                input.CancelRateLast7d >= 0.7f)
            {
                highReasons.Add($"Trong 7 ngày gần đây khách đặt {input.OrdersLast7d} đơn và tỷ lệ hủy rất cao: {input.CancelRateLast7d:P0}.");
            }

            if (input.PhoneUsedCount >= 4)
            {
                highReasons.Add($"Số điện thoại nhận hàng đang được dùng bởi {input.PhoneUsedCount} tài khoản.");
            }

            if (input.AddressUsedCount >= 4)
            {
                highReasons.Add($"Địa chỉ nhận hàng đang được dùng bởi {input.AddressUsedCount} tài khoản.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.CurrentOrderValue >= 5000000)
            {
                highReasons.Add("Tài khoản mới, thanh toán COD và đơn hàng có giá trị rất cao.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.TotalQuantity >= 20)
            {
                highReasons.Add($"Tài khoản mới, thanh toán COD và đặt số lượng rất lớn: {input.TotalQuantity} sản phẩm.");
            }

            // Tổ hợp nguy hiểm: tài khoản mới + COD + đơn cao + thông tin nhận hàng trùng.
            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.CurrentOrderValue >= 3000000 &&
                (input.PhoneUsedCount >= 2 || input.AddressUsedCount >= 2))
            {
                highReasons.Add("Tài khoản mới đặt COD giá trị cao và thông tin nhận hàng trùng với tài khoản khác.");
            }

            // Tổ hợp nguy hiểm: tài khoản mới + COD + đơn cao + nhiều sản phẩm.
            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.CurrentOrderValue >= 3000000 &&
                input.TotalQuantity >= 12 &&
                input.ItemCount >= 5)
            {
                highReasons.Add("Tài khoản mới đặt COD giá trị cao, số lượng lớn và nhiều loại sản phẩm.");
            }

            // Tổ hợp nguy hiểm: đặt nhiều đơn COD gần đây và có hủy.
            if (input.OrdersLast24h >= 2 &&
                input.CancelRateLast24h >= 0.5f &&
                input.IsCod == 1)
            {
                highReasons.Add("Khách đặt nhiều đơn COD trong 24 giờ và có tỷ lệ hủy gần đây cao.");
            }

            if (highReasons.Any())
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "High",
                    Suggestion = GetSuggestion("High"),
                    Reasons = highReasons
                };
            }

            var mediumReasons = new List<string>();
            var weakReasons = new List<string>();

          

            if (input.OrdersLast24h >= 3)
            {
                mediumReasons.Add($"Khách đã đặt {input.OrdersLast24h} đơn trong 24 giờ gần đây.");
            }

            if (input.TotalOrders >= 5 &&
                input.CancelRate >= 0.5f)
            {
                mediumReasons.Add($"Tỷ lệ hủy đơn tổng thể cao: {input.CancelRate:P0}.");
            }

            if (input.OrdersLast7d >= 5 &&
                input.CancelRateLast7d >= 0.5f)
            {
                mediumReasons.Add($"Trong 7 ngày gần đây khách đặt {input.OrdersLast7d} đơn và tỷ lệ hủy là {input.CancelRateLast7d:P0}.");
            }

            if (input.PhoneUsedCount == 3)
            {
                mediumReasons.Add($"Số điện thoại nhận hàng đang được dùng bởi {input.PhoneUsedCount} tài khoản.");
            }

            if (input.AddressUsedCount == 3)
            {
                mediumReasons.Add($"Địa chỉ nhận hàng đang được dùng bởi {input.AddressUsedCount} tài khoản.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.CurrentOrderValue >= 3000000)
            {
                mediumReasons.Add("Tài khoản mới, thanh toán COD và đơn hàng có giá trị cao.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.TotalQuantity >= 15)
            {
                mediumReasons.Add($"Tài khoản mới, thanh toán COD và đặt số lượng lớn: {input.TotalQuantity} sản phẩm.");
            }

            // Giá trị bất thường chỉ xét khi khách đã có đủ lịch sử.
            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 4 &&
                input.CurrentOrderValue >= 2000000)
            {
                mediumReasons.Add("Giá trị đơn hiện tại cao bất thường so với giá trị đơn trung bình của khách.");
            }

            // Tổ hợp Medium: COD + đặt nhiều trong 7 ngày + hủy nhiều.
            if (input.OrdersLast7d >= 4 &&
                input.CancelRateLast7d >= 0.5f &&
                input.IsCod == 1)
            {
                mediumReasons.Add("Khách đặt nhiều đơn COD trong 7 ngày gần đây và có tỷ lệ hủy cao.");
            }

            // Tổ hợp Medium: tài khoản mới + COD + số lượng nhiều + nhiều loại sản phẩm.
            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.TotalQuantity >= 12 &&
                input.ItemCount >= 5)
            {
                mediumReasons.Add("Tài khoản mới đặt COD với số lượng lớn và nhiều loại sản phẩm khác nhau.");
            }

            if (mediumReasons.Any())
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "Medium",
                    Suggestion = GetSuggestion("Medium"),
                    Reasons = mediumReasons
                };
            }

            

            if (input.OrdersLast24h == 2)
            {
                weakReasons.Add("Khách đặt 2 đơn trong 24 giờ gần đây.");
            }

            if (input.PhoneUsedCount == 2)
            {
                weakReasons.Add("Số điện thoại nhận hàng đã xuất hiện ở 2 tài khoản khác nhau.");
            }

            if (input.AddressUsedCount == 2)
            {
                weakReasons.Add("Địa chỉ nhận hàng đã xuất hiện ở 2 tài khoản khác nhau.");
            }

            if (input.StatusChangeCount >= 5)
            {
                weakReasons.Add($"Đơn hàng có số lần thay đổi trạng thái cần chú ý: {input.StatusChangeCount} lần.");
            }

            // Chỉ xét tỷ lệ hủy khi khách đã có đủ lịch sử đơn hàng.
            if (HasEnoughOrderHistory(input) &&
                input.CancelRate >= 0.35f)
            {
                weakReasons.Add($"Tỷ lệ hủy đơn ở mức cần chú ý: {input.CancelRate:P0}.");
            }

            if (input.OrdersLast7d >= 4 &&
                input.CancelRateLast7d >= 0.4f)
            {
                weakReasons.Add($"Trong 7 ngày gần đây tỷ lệ hủy đơn ở mức cần chú ý: {input.CancelRateLast7d:P0}.");
            }

            if (input.TotalOrders >= 3 &&
                input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 3 &&
                input.CurrentOrderValue >= 1500000)
            {
                weakReasons.Add("Giá trị đơn hiện tại cao hơn nhiều so với giá trị đơn trung bình của khách.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.CurrentOrderValue >= 2000000)
            {
                weakReasons.Add("Tài khoản mới, thanh toán COD và đơn hàng có giá trị tương đối cao.");
            }

            if (input.AccountAgeDays <= 7 &&
                input.IsCod == 1 &&
                input.TotalQuantity >= 10)
            {
                weakReasons.Add($"Tài khoản mới, thanh toán COD và đặt số lượng tương đối lớn: {input.TotalQuantity} sản phẩm.");
            }

            
            if (weakReasons.Count >= 2)
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "Medium",
                    Suggestion = GetSuggestion("Medium"),
                    Reasons = weakReasons
                };
            }

          
            if (prediction.IsRisk &&
                weakReasons.Any() &&
                ShouldAcceptAiWarning(input))
            {
                weakReasons.Add("Mô hình AI phát hiện mẫu hành vi cần theo dõi thêm.");

                return new OrderRiskDecision
                {
                    RiskLevel = "Medium",
                    Suggestion = GetSuggestion("Medium"),
                    Reasons = weakReasons
                };
            }

            return LowDecision();
        }

        private OrderRiskDecision LowDecision()
        {
            return new OrderRiskDecision
            {
                RiskLevel = "Low",
                Suggestion = GetSuggestion("Low"),
                Reasons = new List<string>
                {
                    "Không phát hiện dấu hiệu rủi ro đáng chú ý."
                }
            };
        }

        private static bool IsTrustedCustomer(OrderRiskTrainingData input)
        {
            return input.AccountAgeDays >= 30 &&
                   input.TotalOrders >= 3 &&
                   input.CancelRate == 0 &&
                   input.CancelRateLast24h == 0 &&
                   input.CancelRateLast7d == 0 &&
                   input.PhoneUsedCount <= 1 &&
                   input.AddressUsedCount <= 1 &&
                   input.OrdersLast24h <= 1 &&
                   input.StatusChangeCount <= 2 &&
                   input.CurrentOrderValue <= 3000000 &&
                   input.TotalQuantity <= 10 &&
                   input.ItemCount <= 5;
        }

        private static bool IsClearlyLowRiskOrder(OrderRiskTrainingData input)
        {
            return input.CurrentOrderValue < 1500000 &&
                   input.TotalQuantity <= 5 &&
                   input.ItemCount <= 3 &&
                   input.OrdersLast24h <= 1 &&
                   input.OrdersLast7d <= 2 &&
                   input.TotalOrders <= 2 &&
                   input.CancelRate == 0 &&
                   input.CancelRateLast24h == 0 &&
                   input.CancelRateLast7d == 0 &&
                   input.PhoneUsedCount <= 1 &&
                   input.AddressUsedCount <= 1 &&
                   input.StatusChangeCount <= 2;
        }

        private static bool HasEnoughOrderHistory(OrderRiskTrainingData input)
        {
            return input.TotalOrders >= 4;
        }

        private static bool ShouldAcceptAiWarning(OrderRiskTrainingData input)
        {
            if (IsClearlyLowRiskOrder(input) || IsTrustedCustomer(input))
            {
                return false;
            }

            return input.CurrentOrderValue >= 2000000 ||
                   input.TotalQuantity >= 10 ||
                   input.ItemCount >= 5 ||
                   input.OrdersLast24h >= 2 ||
                   input.OrdersLast7d >= 4 ||
                   input.TotalOrders >= 4 ||
                   input.CancelRate >= 0.35f ||
                   input.CancelRateLast24h > 0 ||
                   input.CancelRateLast7d >= 0.4f ||
                   input.PhoneUsedCount >= 2 ||
                   input.AddressUsedCount >= 2 ||
                   input.StatusChangeCount >= 5;
        }
    }
}