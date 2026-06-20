using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.OrderRisk
{
    public class OrderRiskRepository:IOrderRiskRepository
    {
        private readonly AppDatabase _context;

        public OrderRiskRepository(AppDatabase context)
        {
            _context = context;
        }
        public async Task<OrderRiskInputDto?> BuildRiskInputAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            var userId = order.UserId;
            var currentOrderTime = order.CreatedAt;

            var accountAgeDays = order.User != null
                ? Math.Max(0, (currentOrderTime - order.User.CreatedAt).Days)
                : 0;

            // Chỉ lấy lịch sử trước đơn hiện tại.
            // Không tính chính đơn hiện tại vào lịch sử.
            var historyOrders = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId &&
                            o.Id != order.Id &&
                            o.CreatedAt < currentOrderTime);

            var ordersLast24hQuery = historyOrders
                .Where(o => o.CreatedAt >= currentOrderTime.AddHours(-24));

            var ordersLast7dQuery = historyOrders
                .Where(o => o.CreatedAt >= currentOrderTime.AddDays(-7));

            var totalOrders = await historyOrders.CountAsync();

            var ordersLast24h = await ordersLast24hQuery.CountAsync();

            var ordersLast7d = await ordersLast7dQuery.CountAsync();

            var cancelledOrders = await historyOrders
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var completedOrderCount = await historyOrders
                .CountAsync(o => o.Status == OrderStatus.Completed);

            var cancelRate = totalOrders > 0
                ? Math.Round((decimal)cancelledOrders / totalOrders, 3)
                : 0;

            var completionRate = totalOrders > 0
                ? Math.Round((decimal)completedOrderCount / totalOrders, 3)
                : 0;

            var avgOrderValue = completedOrderCount > 0
                ? await historyOrders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .AverageAsync(o => o.TotalAmount)
                : 0;

            var isCod = order.PaymentMethod == PaymentMethod.COD ? 1 : 0;

            var codOrderCount = await historyOrders
                .CountAsync(o => o.PaymentMethod == PaymentMethod.COD);

            var itemCount = order.OrderItems?.Count ?? 0;

            var totalQuantity = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0;

            var isVip = order.User != null && order.User.IsVip ? 1 : 0;

            return new OrderRiskInputDto
            {
                OrderId = order.Id,
                UserId = userId,

                AccountAgeDays = accountAgeDays,

                TotalOrders = totalOrders,
                OrdersLast24h = ordersLast24h,
                OrdersLast7d = ordersLast7d,

                CancelledOrders = cancelledOrders,
                CancelRate = cancelRate,

                CurrentOrderValue = order.TotalAmount,
                AvgOrderValue = avgOrderValue,

                IsCod = isCod,
                CodOrderCount = codOrderCount,

                ItemCount = itemCount,
                TotalQuantity = totalQuantity,

                IsVip = isVip,
                CompletedOrderCount = completedOrderCount,
                CompletionRate = completionRate,

                // Các trường đã bỏ khỏi train/rule.
                // Giữ để đủ DTO nhưng không query thật nữa.
                PhoneUsedCount = 0,
                AddressUsedCount = 0,
                StatusChangeCount = 0,

                CancelledOrdersLast24h = 0,
                CancelRateLast24h = 0,
                CancelledOrdersLast7d = 0,
                CancelRateLast7d = 0
            };
        }
    }
}
