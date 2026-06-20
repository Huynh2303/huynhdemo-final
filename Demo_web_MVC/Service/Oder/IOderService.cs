using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Service.Oder
{
    public interface IOderService
    {
        Task<OderViewModel?> GetOrderDetailAsyncService(int userId, int orderId);
        Task<List<int>> CreateOrderFromCartAsyncService(int userId, string paymentMethod, List<int> selectedCartItemIds);

        //Task<Order> GetOrderByIdAsyncService(int orderId);

        Task<List<OderViewModel>> GetOrdersByUserAsyncService(int userId);
        Task<bool> UpdateOrderStatusAsyncService(int orderId, string status);
        Task<bool> CancelOrderAsyncService(int orderId, int userId);
        //Task<decimal> CalculateOrderTotalAsyncService(int userId);
        Task<CheckoutViewModel>CheckoutAsync(int userId,List<int> selectedCartItemIds);
        Task RemoveCartItemsAsync(List<int> selectedCartItemIds, int userId);
        
        Task<List<OderViewModel>> GetAllOrders(int userId, string status);
        Task<bool> CreateAsync(int orderId, int sellerId);

        Task<bool> DeleteOrderAsync(int orderId, int sellerId);

    }
}
