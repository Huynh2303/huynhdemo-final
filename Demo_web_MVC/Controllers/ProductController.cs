using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Oder;
using Demo_web_MVC.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{

    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOderService _OderService;
        private readonly ICartService _cartService;
        public ProductController(IProductService productService, ICategoryService categoryService,IOderService oderService,ICartService cartService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _OderService = oderService;
            _cartService = cartService;
        }
        private int? GetSellerIdFromClaims()
        {
            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sellerId))
            {
                return null;
            }

            return int.Parse(sellerId);
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
        private async Task<int> GetCartCount()
        {
            var userId = GetUserIdFromClaims(); 
            if (userId == null)
            {
                return 0; 
            }

            var cartItems = await _cartService.GetCartItems(userId.Value);
            return cartItems.Count; 
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            ViewBag.CartCount = await GetCartCount();

            const int pageSize = 24;

            if (categoryId.HasValue)
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId,page,pageSize);

                return View(products);
            }

            var productsPaged = await _productService.getAll(page, pageSize);

            return View(productsPaged);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound("không có id ");

            var productDetails = await _productService.details(id.Value);

            if (productDetails == null)
                return NotFound("không tìm thấy sản phẩm");

            var allProducts = await _productService.GetRelatedProductsAsync();

            productDetails.RelatedProducts = allProducts
                .Where(p => p.Id != productDetails.Id
                         && p.CategoryId == productDetails.CategoryId)
                .OrderBy(x => Guid.NewGuid())
                .Take(4)
                .ToList();

            ViewBag.CartCount = await GetCartCount();

            return View(productDetails);
        }
    }
}
