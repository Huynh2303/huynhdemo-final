using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;
namespace Demo_web_MVC.Service.Shipping
{
    public interface IShippingService
    {
        Task<List<Order>> GetShippingOrdersAsync();
        Task<bool> CompleteDeliveryAsync(int orderId);
        Task<OrderDetailManagementViewModel?> GetShippingOrderDetailAsync(int orderId);
    }
}
