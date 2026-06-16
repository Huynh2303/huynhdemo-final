using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Repository.Carts;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{
    [Authorize(Roles = "ADMIN,STAFF,USER")]
    public class CartController : Controller
    {
        public readonly ICartService _cartService;
        public readonly ILogger<CartController> _logger;
        private readonly IProductService _productService;
        public CartController(ICartService cartService, ILogger<CartController> logger,IProductService productService)
        {
            _cartService = cartService;
            _logger = logger;
            _productService = productService;
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
        public async Task<IActionResult> Index()
        {
            var userid = GetUserIdFromClaims();
            if ( userid == null)
            {
                _logger.LogWarning("khong xac dinh duoc nguoi dung");
                return RedirectToAction("Index", "Product");
            }
            var cartItems = await _cartService.GetCartItems(userid.Value);
            ViewBag.CartCount = cartItems.Count;
           
            return View(cartItems);
        }
        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart (int variantId, int quantity)
        {
            try
            {
                
                var userId = GetUserIdFromClaims();
                if(userId == null)
                {
                    return Unauthorized("Không xác định được người dùng.");
                } 

                var result = await _cartService.AddToCartAsync(userId.Value, variantId, quantity);
                if (result)
                {
                    TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng.";
                    
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể thêm sản phẩm vào giỏ hàng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý yêu cầu.";
            }
            var productId = await _productService.GetProductIdByVariantIdAsync(variantId);
            if (productId == null)
            {
                return RedirectToAction("Index", "Product");
            }
            
            return RedirectToAction("Details", "Product", new { id = productId }); // Điều hướng lại trang giỏ hàng
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Unauthorized("Không xác định được người dùng.");
                }

                var result = await _cartService.RemoveItemAsync(userId.Value, cartItemId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm khỏi giỏ hàng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý yêu cầu.";
            }

            return RedirectToAction("Index", "Cart"); 
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Json(new { success = false, message = "Không xác định được người dùng." });
                }
                // Lấy toàn bộ giỏ hàng để tính tổng
                var cartItems = await _cartService.GetCartItems(userId.Value);
                var cartItemViewModel = new CartItemViewModel
                {
                    Id = cartItemId,
                    Quantity = quantity,
                    Price= cartItems.FirstOrDefault(x => x.Id == cartItemId)?.Price ?? 0 
                };

                var result = await _cartService.UpdateQuantityAsync(userId.Value, cartItemId, cartItemViewModel);

                if (result == null)
                {
                    return Json(new { success = false, message = "Không thể cập nhật số lượng sản phẩm." });
                }

                // Thành tiền của riêng item vừa update
                //var itemTotal = result.Price * result.Quantity;



                //var totalQuantity = cartItems.Sum(x => x.Quantity);
                //var totalAmount = cartItems.Sum(x => x.Price * x.Quantity);
                //return Json(new
                //{
                //    success = true,
                //    message = "Số lượng sản phẩm đã được cập nhật.",
                //    itemTotal = itemTotal,
                //    itemTotalFormatted = itemTotal.ToString("c0", new System.Globalization.CultureInfo("vi-VN")),
                //    totalQuantity = totalQuantity,
                //    totalAmount = totalAmount,
                //    totalAmountFormatted = totalAmount.ToString("c0", new System.Globalization.CultureInfo("vi-VN"))
                //});
                var itemTotal = result.Price * result.Quantity;

                return Json(new
                {
                    success = true,
                    message = "Số lượng sản phẩm đã được cập nhật.",
                    itemTotal = itemTotal,
                    itemTotalFormatted = itemTotal.ToString("c0", new System.Globalization.CultureInfo("vi-VN"))
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng sản phẩm trong giỏ hàng.");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xử lý yêu cầu." });
            }
        }
    }
}
