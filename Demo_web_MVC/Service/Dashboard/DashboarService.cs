using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Repository.Dashboard;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Service.Dashboard
{
    public class DashboarService:IDashboarService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboarService> _logger;
        public DashboarService ( IDashboardRepository dashboardRepository, ILogger<DashboarService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }
        public async Task<DashboardViewModel> GetOrdersAndProductsAsync(int sellerId,
    int orderPage = 1,
    int productPage = 1,
    int pageSize = 5)
        {
            try
            {
                var result = await _dashboardRepository.GetOrdersAndProductsAsync(sellerId,
        orderPage,
        productPage,
        pageSize);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Có lỗi khi lấy dữ liệu dashboard: {ex.Message}");
                return null!; // Hoặc trả về một đối tượng mặc định
            }
        }
        public async Task<ProductsManagerViewModel> GetProductsManagerAsync(
    int sellerId,
    int page = 1,
    int pageSize = 10)
        {
            return await _dashboardRepository.GetProductsManagerAsync(
                sellerId,
                page,
                pageSize
            );
        }
        public async Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId, int sellerId)
        {
            return await _dashboardRepository.GetDetailsOrderDashboardViewmodelAsync(orderId,  sellerId);
        }
        public async Task<StatisticsViewModel> GetStatisticsAsync(int sellerId)
        {
            
            var statistics = await _dashboardRepository.GetDashboardStatisticsAsync( sellerId);
            return statistics;
        }
    }
}
