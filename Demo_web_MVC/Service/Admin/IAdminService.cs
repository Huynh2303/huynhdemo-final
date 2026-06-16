using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;

namespace Demo_web_MVC.Service.Admin
{
    public interface IAdminService
    {
        Task<AdminViewModel> GetAdminDashboardAsync();
        Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize);
        Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize);
        Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize);
        Task<CategoryManagementViewModel> GetCategoryManagementAsync(int page, int pageSize);
        Task<OrderDetailManagementViewModel?> GetOrderDetailManagementAsync(int orderId);
        Task<ProductManagerDetailViewModel?> GetProductManagerDetailAsync(int productId);
        Task<bool> DeleteProductByAdminAsync(int productId);
        //
        Task<bool> ConfirmOrderAsync(int orderId);

        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        Task<bool> CancelOrderAsync(int orderId);

        Task<FraudAnalysis?> GetOrderRiskAnalysisAsync(int orderId);
        //
        Task<bool> AddCategoryAsync(CategoryManagementViewModel model);

        Task<bool> UpdateCategoryAsync(CategoryManagementViewModel model);

        Task<bool> DeleteCategoryAsync(int categoryId);
        Task<CategoryManagementViewModel?> GetCategoryByIdAsync(int id);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<bool> ChangeUserToStaffAsync(int userId);
    }
}
