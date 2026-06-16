using Demo_web_MVC.Models.ViewModel.Dashboard;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Service.Dashboard
{
    public interface IDashboarService
    {
        Task<DashboardViewModel> GetOrdersAndProductsAsync(
    int sellerId,
    int orderPage = 1,
    int productPage = 1,
    int pageSize = 5);
        Task<ProductsManagerViewModel> GetProductsManagerAsync(
    int sellerId,
    int page = 1,
    int pageSize = 10);
        Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId, int sellerId);
        Task<StatisticsViewModel> GetStatisticsAsync(int sellerId);
    }
}
