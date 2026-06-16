using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Repository.Oder
{
    public interface IOderRepository
    {
        Task<int> CreateOrderFromCartAsync(int userId, string paymentMethod, List<int> selectedCartItemIds);
        Task<Order> GetOrderByIdAsync( int orderId);
        Task<OderViewModel?> GetOrderDetailAsync(int userId, int orderId);
        Task<List<OderViewModel>> GetOrdersByUserAsync(int userId);
        Task<List<OderViewModel>> GetAllOrders(int userId, string status);

        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId, int userId);

        //Task<bool> CreatePaymentAsync(int orderId, string paymentMethod, decimal amount, string status);

        Task<List<OderViewModel>> GetAllOrderIdsAsync(int orderId);
        Task<decimal> CalculateOrderTotalAsync(int userId);
        //Task<List<OrderItemViewModel>> GetOrderItemsAsync(int orderId);
        Task<CheckoutViewModel> CheckOutAsync (int userId,List<int> selectedCartItemIds);
        Task RemoveCartItemsAsync(List<int> selectedCartItemIds, int userId);
        Task<bool> DeleteOrderAsync(int orderId, int sellerId); // Xóa đơn hàng
        Task<bool> CreateAsync(int orderId, int sellerId);

    }
}
