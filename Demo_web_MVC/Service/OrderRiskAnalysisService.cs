using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Repository.OrderRisk;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Demo_web_MVC.Service
{
    public class OrderRiskAnalysisService
    {
        private readonly AppDatabase _context;
        private readonly OrderRiskPredictor _orderRiskPredictor;
        private readonly IOrderRiskRepository _orderRiskRepository;

        public OrderRiskAnalysisService(
            AppDatabase context,
            OrderRiskPredictor orderRiskPredictor,
            IOrderRiskRepository orderRiskRepository)
        {
            _context = context;
            _orderRiskPredictor = orderRiskPredictor;
            _orderRiskRepository = orderRiskRepository;
        }

        public async Task AnalyzeOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return;
            }

            var dto = await _orderRiskRepository.BuildRiskInputAsync(order.Id);
            if (dto == null) {
                return;
            }
            var input = new OrderRiskTrainingData
            {
                AccountAgeDays = dto.AccountAgeDays,
                TotalOrders = dto.TotalOrders,
                OrdersLast24h = dto.OrdersLast24h,
                OrdersLast7d = dto.OrdersLast7d,
                CancelledOrders = dto.CancelledOrders,
                CancelRate = (float)dto.CancelRate,
                CurrentOrderValue = (float)dto.CurrentOrderValue,
                AvgOrderValue = (float)dto.AvgOrderValue,
                IsCod = dto.IsCod,
                CodOrderCount = dto.CodOrderCount,
                PhoneUsedCount = dto.PhoneUsedCount,
                AddressUsedCount = dto.AddressUsedCount,
                ItemCount = dto.ItemCount,
                TotalQuantity = dto.TotalQuantity,
                StatusChangeCount = dto.StatusChangeCount,
                CancelledOrdersLast24h = dto.CancelledOrdersLast24h,
                CancelRateLast24h = (float)dto.CancelRateLast24h,
                CancelledOrdersLast7d = dto.CancelledOrdersLast7d,
                CancelRateLast7d = (float)dto.CancelRateLast7d
            };
            var prediction = _orderRiskPredictor.Predict(input);

            var decision = _orderRiskPredictor.GetRiskDecision(input, prediction);

            var fraudAnalysis = new FraudAnalysis
            {
                OrderId = order.Id,
                RiskScore = (decimal)prediction.Score,
                ModelName = "FastForest_order_risk_v4_3000",
                CreatedAt = DateTime.Now,
                InputSnapshot = JsonSerializer.Serialize(input),
                RiskLevel = decision.RiskLevel,
                RiskReasons = JsonSerializer.Serialize(decision.Reasons)
            };

            _context.FraudAnalyses.Add(fraudAnalysis);

            await _context.SaveChangesAsync();
        }
    }
}
