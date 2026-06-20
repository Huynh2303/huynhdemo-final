using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Service.Oder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    [Authorize(Roles = "ADMIN,STAFF,USER")]
    
    public class OderController : Controller
    {
        private readonly IOderService _service;
        private readonly ILogger<OderController> _logger;
        public OderController (IOderService service, ILogger<OderController> logger)
        {
            _service = service;
            _logger = logger;
        }
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
        
        public async Task<IActionResult> Details (int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("Không xác định được người dùng.");
            }
            var result = await _service.GetOrderDetailAsyncService(userId.Value, orderId);
            return View(result);
        }
        public IActionResult CreateOrder()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(string paymentMethod, List<int> selectedCartItemIds)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (userId == null)
                {
                    return RedirectToAction("Login", "User");
                }

                var orderIds = await _service.CreateOrderFromCartAsyncService(
                    userId.Value,
                    paymentMethod,
                    selectedCartItemIds
                );

                if (orderIds == null || !orderIds.Any())
                {
                    TempData["ErrorMessage"] = "Không tạo được đơn hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                TempData["SuccessMessage"] = "Đặt hàng thành công.";

                return RedirectToAction("Details", "Oder", new { orderId = orderIds.First() });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng");

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var orders = await _service.GetOrdersByUserAsyncService(userId.Value);

            return View(orders);
        }
        [Authorize(Roles = "ADMIN,STAFF")]
        public IActionResult UpdateStatus()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var result = await _service.UpdateOrderStatusAsyncService(orderId, status);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Không cập nhật được trạng thái.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Cập nhật thành công.";
                }

                return RedirectToAction("Details", "Oder", new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi update status");

                TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                return RedirectToAction("Details", "Oder", new { orderId });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (userId == null)
                {
                    return RedirectToAction("Login", "User");
                }

                var result = await _service.CancelOrderAsyncService(orderId, userId.Value);

                if (result)
                {
                    TempData["SuccessMessage"] = "Đã hủy đơn hàng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn.";
                }

                return RedirectToAction("Index", "Oder", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hủy đơn");

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Oder", new { orderId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Checkout(List<int> selectedCartItemIds)
        {
            var userId = GetUserIdFromClaims(); 
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán."; 
                return RedirectToAction("Index", "Cart"); 
            }
            
            var model = await _service.CheckoutAsync(userId.Value, selectedCartItemIds);

            
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Index(string status, bool onlyList = false)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return NotFound("Không có User nào");
            }
            if (string.IsNullOrEmpty(status))
            {
                status = "All"; 
            }
            List<OderViewModel> orders;

            orders = await _service.GetAllOrders(userId.Value, status);

            _logger.LogInformation("Đã lấy {OrderCount} đơn hàng cho UserId: {UserId}", orders.Count, userId.Value);
            if (onlyList)
            {
                return PartialView("_OrderList", orders);
            }

            return PartialView( "Index",orders);
        }

    }
}
