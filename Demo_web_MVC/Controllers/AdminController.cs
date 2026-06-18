using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Service.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminDashboardService;

        public AdminController(IAdminService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }
        // xin chào
        public IActionResult Index()
        {
            
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            

            var model = await _adminDashboardService.GetAdminDashboardAsync();

            return PartialView("Dashboard", model);
        }
        public async Task<IActionResult> OrderManagement(int page = 1)
        {
            int pageSize = 10;
            var model = await _adminDashboardService
        .GetOrderManagementAsync(page, pageSize);
            return PartialView("OrderManagement", model);
        }
        public async Task<IActionResult> ProductManagement(
            int page = 1)
        {
            int pageSize = 10;

            var model = await _adminDashboardService
                .GetProductManagementAsync(page, pageSize);

            return PartialView("ProductManagement", model);
        }
        public async Task<IActionResult> UserManagement(int page = 1)
        {
            int pageSize = 10;
            var model = await _adminDashboardService.GetUserManagementAsync(page, pageSize);

            return PartialView("UserManagement", model);
        }
        public async Task<IActionResult> CategoryManagement(int page = 1)
        {
            int pageSize = 5;

            var model = await _adminDashboardService.GetCategoryManagementAsync(page, pageSize);

            return PartialView("CategoryManagement", model);
        }
        public async Task<IActionResult> OrderDetailManagement(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderDetailManagementAsync(orderId);

            return PartialView("OrderDetailManagement", model);
        }
        public async Task<IActionResult> ProductManagerDetail(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var model = await _adminDashboardService.GetProductManagerDetailAsync(id);

            return PartialView("ProductManagerDetail", model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductByAdmin(int productId)
        {
            await _adminDashboardService.DeleteProductByAdminAsync(productId);

            var model = await _adminDashboardService.GetProductManagementAsync(1, 10);

            return PartialView("ProductManagement", model);
        }
        // 
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            await _adminDashboardService.ConfirmOrderAsync(orderId);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            await _adminDashboardService.CancelOrderAsync(orderId);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateOrderStatusModal(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderDetailManagementAsync(orderId);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("UpdateOrderStatus", model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            await _adminDashboardService
                .UpdateOrderStatusAsync(orderId, newStatus);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        public async Task<IActionResult> OrderRiskAnalysis(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderRiskAnalysisAsync(orderId);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("OrderRiskAnalysis", model);
        }
        //
        [HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            var model = await _adminDashboardService
                .GetCategoryManagementAsync(1, 5);

            return PartialView("_CategoryForm", model);
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryManagementViewModel model)
        {
            await _adminDashboardService.AddCategoryAsync(model);

            var newModel = await _adminDashboardService
                .GetCategoryManagementAsync(1, 5);

            return PartialView("CategoryManagement", newModel);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var model = await _adminDashboardService
                .GetCategoryByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("_CategoryForm", model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(CategoryManagementViewModel model)
        {
            await _adminDashboardService.UpdateCategoryAsync(model);

            var newModel = await _adminDashboardService
                .GetCategoryManagementAsync(1, 5);

            return PartialView("CategoryManagement", newModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            await _adminDashboardService.DeleteCategoryAsync(categoryId);

            var newModel = await _adminDashboardService
                .GetCategoryManagementAsync(1, 5);

            return PartialView("CategoryManagement", newModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(int userId, bool isActive)
        {
            await _adminDashboardService
                .UpdateUserStatusAsync(userId, isActive);

            var model = await _adminDashboardService
                .GetUserManagementAsync(1, 10);

            return PartialView("UserManagement", model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserToStaff(int userId)
        {
            var result = await _adminDashboardService
                .ChangeUserToStaffAsync(userId);

            if (!result)
            {
                return BadRequest();
            }

            var model = await _adminDashboardService
                .GetUserManagementAsync(1, 10);

            return PartialView("UserManagement", model);
        }
    }
}
