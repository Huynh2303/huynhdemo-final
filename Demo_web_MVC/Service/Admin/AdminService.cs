using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.Admin;
using System;

namespace Demo_web_MVC.Service.Admin
{
    public class AdminService: IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IAdminRepository adminRepository,
            ILogger<AdminService> logger)
        {
            _adminRepository = adminRepository;
            _logger = logger;
        }
        public async Task<AdminViewModel> GetAdminDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Service bắt đầu lấy dashboard admin...");

                var dashboard = await _adminRepository.GetAdminDashboardAsync();
                if (dashboard == null)
                {
                    return new AdminViewModel();
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi service khi lấy dashboard admin.");
                throw;
            }
        }
        public async Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize)
        {
            var model = await _adminRepository
            .GetOrderManagementAsync(page, pageSize);

            if (model == null)
            {
                return new OderManagementViewModel
                {
                    Orders = new PaginatedList<OderViewModel>(
                        new List<OderViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }

            return model;
        }
        public async Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var model = await _adminRepository
                .GetProductManagementAsync(page, pageSize);

            if (model == null)
            {
                return new ProductManagementViewModel
                {
                    Products = new PaginatedList<ProductViewModel>(
                        new List<ProductViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }
            model.Products ??= new PaginatedList<ProductViewModel>(
                new List<ProductViewModel>(),
                0,
                page,
                pageSize
            );

            return model;
        }
        public async Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var model = await _adminRepository.GetUserManagementAsync(page, pageSize);

            if (model == null)
            {
                return new UserManagementViewModel
                {
                    Users = new PaginatedList<UserItemViewModel>(
                        new List<UserItemViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }

            return model;
        }
        public async Task<CategoryManagementViewModel> GetCategoryManagementAsync(int page, int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var model = await _adminRepository.GetCategoryManagementAsync(page, pageSize);

            if (model == null)
            {
                return new CategoryManagementViewModel
                {
                    Categories = new PaginatedList<CategoryViewModel>(
                        new List<CategoryViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }

            model.Categories ??= new PaginatedList<CategoryViewModel>(
                new List<CategoryViewModel>(),
                0,
                page,
                pageSize
            );

            return model;
        }
        public async Task<OrderDetailManagementViewModel?> GetOrderDetailManagementAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            var model = await _adminRepository.GetOrderDetailManagementAsync(orderId);

            if (model == null)
            {
                return null;
            }

            model.OrderDetails ??= new List<OderItemViewModel>();

            return model;
        }
        public async Task<ProductManagerDetailViewModel?> GetProductManagerDetailAsync(int productId)
        {
            if (productId <= 0)
            {
                return null;
            }

            var product = await _adminRepository.GetProductManagerDetailAsync(productId);

            if (product == null)
            {
                return null;
            }

            product.Variants ??= new List<ProductVariantsViewModel>();
            product.ProductImages ??= new List<string>();

            product.TotalVariants = product.Variants.Count;
            product.TotalStock = product.Variants.Sum(v => v.Stock);

            product.MinPrice = product.Variants.Any()
                ? product.Variants.Min(v => v.Price)
                : 0;

            product.MaxPrice = product.Variants.Any()
                ? product.Variants.Max(v => v.Price)
                : 0;

            return product;
        }
        public async Task<bool> DeleteProductByAdminAsync(int productId)
        {
            if (productId <= 0)
            {
                return false;
            }

            var result = await _adminRepository.DeleteProductByAdminAsync(productId);

            return result;
        }
        //
        public async Task<bool> ConfirmOrderAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return false;
            }

            return await _adminRepository.ConfirmOrderAsync(orderId);
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            if (orderId <= 0)
            {
                return false;
            }

            return await _adminRepository.UpdateOrderStatusAsync(orderId, newStatus);
        }
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return false;
            }

            return await _adminRepository.CancelOrderAsync(orderId);
        }
        public async Task<FraudAnalysis?> GetOrderRiskAnalysisAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            return await _adminRepository.GetOrderRiskAnalysisAsync(orderId);
        }
        //
        public async Task<bool> AddCategoryAsync(CategoryManagementViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return false;
            }

            return await _adminRepository.AddCategoryAsync(model);
        }

        public async Task<bool> UpdateCategoryAsync(CategoryManagementViewModel model)
        {
            if (model == null || model.Id == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return false;
            }

            // Không cho chọn chính nó làm danh mục cha
            if (model.ParentId == model.Id)
            {
                return false;
            }

            return await _adminRepository.UpdateCategoryAsync(model);
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                return false;
            }

            return await _adminRepository.DeleteCategoryAsync(categoryId);
        }
        public async Task<CategoryManagementViewModel?> GetCategoryByIdAsync(int id)
        {
            return await _adminRepository.GetCategoryByIdAsync(id);
        }
        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            return await _adminRepository
                .UpdateUserStatusAsync(userId, isActive);
        }
        public async Task<bool> ChangeUserToStaffAsync(int userId)
        {
            if (userId <= 0)
            {
                return false;
            }

            return await _adminRepository.ChangeUserToStaffAsync(userId);
        }
    }
}
