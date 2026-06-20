using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Repository.Oder;

namespace Demo_web_MVC.Service.Oder
{
    public class OderService:IOderService
    {
        private readonly IOderRepository _oderRepository;
        private readonly ILogger<OderService> _logger;
        private readonly OrderRiskAnalysisService _orderRiskAnalysisService;
        public OderService (IOderRepository oderRepository,ILogger<OderService> logger, OrderRiskAnalysisService orderRiskAnalysisService)
        {
            _oderRepository = oderRepository;
            _logger = logger;
            _orderRiskAnalysisService= orderRiskAnalysisService;
        }
        public async Task<List<int>> CreateOrderFromCartAsyncService(
    int userId,
    string paymentMethod,
    List<int> selectedCartItemIds)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId không hợp lệ");
            }

            if (string.IsNullOrEmpty(paymentMethod))
            {
                throw new ArgumentException("Payment method không được để trống");
            }

            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                throw new ArgumentException("Chưa chọn sản phẩm để đặt hàng");
            }

            if (!Enum.TryParse(paymentMethod, true, out PaymentMethod method))
            {
                throw new ArgumentException("Payment method không hợp lệ");
            }

            try
            {
                var orderIds = await _oderRepository.CreateOrderFromCartAsync(
                    userId,
                    paymentMethod,
                    selectedCartItemIds
                );

                if (orderIds == null || !orderIds.Any())
                {
                    throw new InvalidOperationException("Không tạo được đơn hàng nào.");
                }

                _logger.LogInformation(
                    "Tạo đơn hàng thành công. UserId={UserId}, OrderIds={OrderIds}",
                    userId,
                    string.Join(",", orderIds)
                );

                foreach (var orderId in orderIds)
                {
                    await _orderRiskAnalysisService.AnalyzeOrderAsync(orderId);
                }

                return orderIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng. UserId={UserId}", userId);
                throw;
            }
        }
        public async Task<OderViewModel?> GetOrderDetailAsyncService( int userId, int orderId)
        {
            var result = await _oderRepository.GetOrderDetailAsync( userId,orderId);
            
            return result;
        }
        public async Task<List<OderViewModel>> GetOrdersByUserAsyncService(int userId)
        {
            // 1. Validate
            if (userId <= 0)
            {
                throw new ArgumentException("UserId không hợp lệ");
            }

            try
            {
                // 2. Gọi repository
                var orders = await _oderRepository.GetOrdersByUserAsync(userId);

                // 3. Check null / empty
                if (orders == null || !orders.Any())
                {
                    _logger.LogWarning("Không có đơn hàng cho userId={UserId}", userId);
                    return new List<OderViewModel>(); // trả list rỗng (đừng trả null)
                }

                // 4. Có thể xử lý thêm logic tại đây nếu cần

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng userId={UserId}", userId);
                throw;
            }
        }
        public async Task<bool> UpdateOrderStatusAsyncService(int orderId, string status)
        {
            // 1. Validate
            if (orderId <= 0)
            {
                throw new ArgumentException("OrderId không hợp lệ");
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("Status không được để trống");
            }

            // 2. Convert string → enum (BEST PRACTICE)
            if (!Enum.TryParse(status, true, out OrderStatus orderStatus))
            {
                throw new ArgumentException("Status không hợp lệ");
            }

            try
            {
                // 3. Gọi repository
                var result = await _oderRepository.UpdateOrderStatusAsync(orderId, status);

                if (!result)
                {
                    _logger.LogWarning("Không cập nhật được trạng thái đơn hàng. OrderId={OrderId}", orderId);
                }



                _logger.LogInformation("Cập nhật trạng thái đơn hàng thành công. OrderId={OrderId}, Status={Status}", orderId, orderStatus);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng. OrderId={OrderId}", orderId);
                throw;
            }
        }
        public async Task<bool> CancelOrderAsyncService(int orderId, int userId)
        {
            // 1. Validate
            if (orderId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Dữ liệu không hợp lệ");
            }

            try
            {
                // 2. Lấy order
                var order = await _oderRepository.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    throw new InvalidOperationException("Không tìm thấy đơn hàng");
                }

                // 3. Check quyền (rất quan trọng)
                if (order.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền hủy đơn này");
                }

                // 4. Check trạng thái (business rule)
                if (order.Status != OrderStatus.Pending)
                {
                    throw new InvalidOperationException("Chỉ được hủy đơn khi đang chờ xử lý");
                }

                // 5. Update trạng thái → Cancelled
                var result = await _oderRepository.CancelOrderAsync(orderId, userId);

                if (!result)
                {
                    _logger.LogWarning("Hủy đơn thất bại. OrderId={OrderId}", orderId);
                }

                _logger.LogInformation("Hủy đơn thành công. OrderId={OrderId}", orderId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy đơn. OrderId={OrderId}", orderId);
                throw;
            }
        }

        public async Task<CheckoutViewModel> CheckoutAsync(int userId, List<int> selectedCartItemIds)
        {
            return await _oderRepository.CheckOutAsync(userId , selectedCartItemIds);
        }
        public async Task RemoveCartItemsAsync(List<int> selectedCartItemIds, int userId)
        {
             await _oderRepository.RemoveCartItemsAsync(selectedCartItemIds, userId);
        }
        
        public async Task<List<OderViewModel>> GetAllOrders(int userId, string status)
        {
            var orders = await _oderRepository.GetAllOrders(userId,  status);

           
            return orders;
        }
        public async Task<bool> CreateAsync(int orderId, int sellerId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("OrderId không hợp lệ: {OrderId}", orderId);
                return false;
            }

            if (sellerId <= 0)
            {
                _logger.LogWarning("SellerId không hợp lệ: {SellerId}", sellerId);
                return false;
            }

            var result = await _oderRepository
                .CreateAsync(orderId, sellerId);

            if (!result)
            {
                _logger.LogWarning(
                    "Service: Seller {SellerId} không thể cập nhật đơn hàng sang Shipping. orderId={OrderId}",
                    sellerId,
                    orderId);

                return false;
            }

            _logger.LogInformation(
                "Service: Seller {SellerId} cập nhật đơn hàng sang Shipping thành công. orderId={OrderId}",
                sellerId,
                orderId);

            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId, int sellerId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("OrderId không hợp lệ: {OrderId}", orderId);
                return false;
            }

            if (sellerId <= 0)
            {
                _logger.LogWarning("SellerId không hợp lệ: {SellerId}", sellerId);
                return false;
            }

            var result = await _oderRepository
                .DeleteOrderAsync(orderId, sellerId);

            if (!result)
            {
                _logger.LogWarning(
                    "Service: Seller {SellerId} không thể hủy đơn hàng. orderId={OrderId}",
                    sellerId,
                    orderId);

                return false;
            }

            _logger.LogInformation(
                "Service: Seller {SellerId} hủy đơn hàng thành công. orderId={OrderId}",
                sellerId,
                orderId);

            return true;
        }

    }
}
