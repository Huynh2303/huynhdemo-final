using Demo_web_MVC.Models;

namespace Demo_web_MVC.Repository.OrderRisk
{
    public interface IOrderRiskRepository
    {
        Task<OrderRiskInputDto?> BuildRiskInputAsync(int orderId);
    }
}
