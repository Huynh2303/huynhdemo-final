using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Repository.Addresss;
using Demo_web_MVC.Service.Address;
using Demo_web_MVC.Service.Cart;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IAddressService _addressService;
        private readonly ICartService _cartService;
        public HomeController(ILogger<HomeController> logger, IAddressService address ,ICartService cartService)
        {
            _logger = logger;
            _addressService = address;
            _cartService = cartService;
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
        public async  Task<IActionResult> Index()
        {
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult TestSession()
        {
            HttpContext.Session.SetString("TestKey", "Hello Session SQL");

            return Content("Session saved");
        }
        public IActionResult ReadSession()
        {
            var value = HttpContext.Session.GetString("TestKey");

            return Content(value ?? "NULL");

        }
    }
}
