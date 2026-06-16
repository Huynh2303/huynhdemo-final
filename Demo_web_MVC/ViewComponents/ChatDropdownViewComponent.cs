using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Chat;
using Demo_web_MVC.Service.Chat;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.ViewComponents
{
    public class ChatDropdownViewComponent : ViewComponent
    {
        private readonly IChatService _chatService;

        public ChatDropdownViewComponent(IChatService chatService)
        {
            _chatService = chatService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return View(new List<ChatDropdownViewModel>());
            }

            var userId = int.Parse(
                UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var chats = await _chatService
                .GetChatDropdownAsync(userId);

            return View(chats);
        }
    }
}
