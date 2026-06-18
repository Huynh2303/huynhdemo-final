using Demo_web_MVC.Service.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    //[Authorize(Roles = "STAFF,ADMIN")]
    public class ShippingController : Controller
    {
        private readonly IShippingService _shippingService;
        private readonly ILogger<ShippingController> _logger;

        public ShippingController(
            IShippingService shippingService,
            ILogger<ShippingController> logger)
        {
            _shippingService = shippingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _shippingService.GetShippingOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách đơn đang giao.");

                TempData["ErrorMessage"] = "Không thể tải danh sách đơn đang giao.";
                return View(new List<Models.Order>());
            }
        }

        public async Task<IActionResult> Detail(int orderId)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var orderDetail = await _shippingService.GetShippingOrderDetailAsync(orderId);

                if (orderDetail == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng đang giao.";
                    return RedirectToAction(nameof(Index));
                }

                return View(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết đơn hàng. OrderId={OrderId}", orderId);

                TempData["ErrorMessage"] = "Không thể xem chi tiết đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int orderId)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _shippingService.CompleteDeliveryAsync(orderId);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Không thể hoàn thành đơn hàng này.";
                    return RedirectToAction(nameof(Detail), new { orderId });
                }

                TempData["SuccessMessage"] = "Đơn hàng đã được giao thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành đơn hàng. OrderId={OrderId}", orderId);

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hoàn thành đơn hàng.";
                return RedirectToAction(nameof(Detail), new { orderId });
            }
        }
    }
}