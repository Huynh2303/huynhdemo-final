using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Repository.Oder;
using Demo_web_MVC.Service.Admin;

namespace Demo_web_MVC.Service.Shipping
{
    public class ShippingService : IShippingService
    {
        private readonly IOderRepository _oderRepository;
        private readonly IAdminService _adminService;
        private readonly ILogger<ShippingService> _logger;

        public ShippingService(
            IOderRepository oderRepository,
            IAdminService adminService,
            ILogger<ShippingService> logger)
        {
            _oderRepository = oderRepository;
            _adminService = adminService;
            _logger = logger;
        }

        public async Task<List<Order>> GetShippingOrdersAsync()
        {
            return await _oderRepository.GetShippingOrdersAsync();
        }

        public async Task<OrderDetailManagementViewModel?> GetShippingOrderDetailAsync(int orderId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("OrderId không hợp lệ: {OrderId}", orderId);
                return null;
            }

            var orderDetail = await _adminService.GetOrderDetailManagementAsync(orderId);

            if (orderDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết đơn hàng. OrderId={OrderId}", orderId);
                return null;
            }

            if (orderDetail.Order.Status != OrderStatus.Shipping)
            {
                _logger.LogWarning(
                    "Đơn hàng không ở trạng thái Shipping. OrderId={OrderId}, Status={Status}",
                    orderId,
                    orderDetail.Order.Status
                );

                return null;
            }

            return orderDetail;
        }

        public async Task<bool> CompleteDeliveryAsync(int orderId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("OrderId không hợp lệ: {OrderId}", orderId);
                return false;
            }

            try
            {
                var order = await _oderRepository.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng. OrderId={OrderId}", orderId);
                    return false;
                }

                if (!order.CanCompleteDelivery())
                {
                    _logger.LogWarning(
                        "Không thể hoàn thành đơn. OrderId={OrderId}, Status={Status}",
                        orderId,
                        order.Status
                    );

                    return false;
                }

                order.CompleteDelivery();

                var result = await _oderRepository.UpdateOrderStatusAsync(
                    orderId,
                    order.Status.ToString()
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành giao hàng. OrderId={OrderId}", orderId);
                return false;
            }
        }
    }
}