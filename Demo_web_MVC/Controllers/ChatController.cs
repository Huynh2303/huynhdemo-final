using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;


namespace Demo_web_MVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ICartService _cartService;
        public ChatController(IChatService chatService, IHubContext<ChatHub> chatHub, ICartService cartService)
        {
            _chatService = chatService;
            _chatHub = chatHub;
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
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var role = GetRole();

            var conversations = await _chatService.GetConversationsAsync(userId, role);
            ViewBag.CartCount = await GetCartCount();
            return View(conversations);
        }
        public async Task<IActionResult> ConversationList()
        {
            var userId = GetUserId();
            var role = GetRole();

            var conversations = await _chatService
                .GetConversationsAsync(userId, role);

            return PartialView("_ConversationList", conversations);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetUserId();
            var role = GetRole();
            ViewBag.CartCount = await GetCartCount();
            var conversation = await _chatService.GetConversationDetailAsync(id, userId, role);

            if (conversation == null)
                return NotFound();

            return View(conversation);
        }

        public async Task<IActionResult> Support()
        {
            var userId = GetUserId();

            var conversation = await _chatService
                .GetOrCreateSystemSupportConversationAsync(userId);

            return RedirectToAction("Detail", new { id = conversation.Id });
        }

        public async Task<IActionResult> OrderSeller(int orderId, int sellerId)
        {
            var userId = GetUserId();

            try
            {
                var conversation = await _chatService
                    .GetOrCreateOrderSellerConversationAsync(orderId, userId, sellerId);

                return RedirectToAction("Detail", new { id = conversation.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Details", "Oder", new { id = orderId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var userId = GetUserId();
            var role = GetRole();

            var message = await _chatService.SendMessageAsync(
                conversationId,
                userId,
                role,
                content);

            await _chatHub.Clients
                .Group($"conversation-{conversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    conversationId = conversationId,
                    senderId = message.SenderId,
                    senderName = User.Identity?.Name ?? "Người dùng",
                    content = message.Content,
                    createdAt = message.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                });

            return Ok();
        }

        private int GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Không tìm thấy UserId.");

            return int.Parse(userId);
        }

        private string GetRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }
        public async Task<IActionResult> CustomerChats()
        {
            var sellerId = GetUserId();

            var conversations =
                await _chatService
                    .GetOrderSellerConversationsAsync(sellerId);

            return View(conversations);
        }
        public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdValue, out int userId))
                {
                    ViewBag.ChatDropdown =
                        await _chatService.GetChatDropdownAsync(userId);
                }
            }

            await next();
        }
    }
}
