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
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            var now = DateTime.Now;
            var userId = order.UserId;

            var accountAgeDays = (now - order.User.CreatedAt).Days;

            var userOrders = _context.Orders
                .Where(o => o.UserId == userId);

            var ordersLast24hQuery = userOrders
                .Where(o => o.CreatedAt >= now.AddHours(-24));

            var ordersLast7dQuery = userOrders
                .Where(o => o.CreatedAt >= now.AddDays(-7));

            var totalOrders = await userOrders.CountAsync();

            var ordersLast24h = await ordersLast24hQuery.CountAsync();

            var ordersLast7d = await ordersLast7dQuery.CountAsync();

            var cancelledOrders = await userOrders
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var cancelledOrdersLast24h = await ordersLast24hQuery
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var cancelledOrdersLast7d = await ordersLast7dQuery
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var cancelRate = totalOrders > 0
                ? Math.Round((decimal)cancelledOrders / totalOrders, 3)
                : 0;

            var cancelRateLast24h = ordersLast24h > 0
                ? Math.Round((decimal)cancelledOrdersLast24h / ordersLast24h, 3)
                : 0;

            var cancelRateLast7d = ordersLast7d > 0
                ? Math.Round((decimal)cancelledOrdersLast7d / ordersLast7d, 3)
                : 0;

            var avgOrderValue = totalOrders > 0
                ? await userOrders.AverageAsync(o => o.TotalAmount)
                : 0;

            var isCod = order.PaymentMethod == PaymentMethod.COD ? 1 : 0;

            var codOrderCount = await userOrders
                .CountAsync(o => o.PaymentMethod == PaymentMethod.COD);

            var itemCount = order.OrderItems.Count;

            var totalQuantity = order.OrderItems.Sum(oi => oi.Quantity);

            var statusChangeCount = await _context.OrderLogs
                .CountAsync(ol => ol.OrderId == order.Id);

            var phone = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Id)
                .Select(a => a.PhoneNumber)
                .FirstOrDefaultAsync();

            var address = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Id)
                .Select(a => a.AddressLine)
                .FirstOrDefaultAsync();

            var phoneUsedCount = string.IsNullOrEmpty(phone)
                ? 0
                : await _context.Addresses
                    .Where(a => a.PhoneNumber == phone)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

            var addressUsedCount = string.IsNullOrEmpty(address)
                ? 0
                : await _context.Addresses
                    .Where(a => a.AddressLine == address)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

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
                PhoneUsedCount = phoneUsedCount,
                AddressUsedCount = addressUsedCount,
                ItemCount = itemCount,
                TotalQuantity = totalQuantity,
                StatusChangeCount = statusChangeCount,
                CancelledOrdersLast24h = cancelledOrdersLast24h,
                CancelRateLast24h = cancelRateLast24h,
                CancelledOrdersLast7d = cancelledOrdersLast7d,
                CancelRateLast7d = cancelRateLast7d
            };
        }
    }
}
