using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Service.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.ViewComponents
{
    public class NotificationsDropdownViewComponent: ViewComponent
    {
        private readonly INotificationsService _notificationService;

        public NotificationsDropdownViewComponent(
            INotificationsService notificationService)
        {
            _notificationService = notificationService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View(new List<NotificationViewModel>());
            }

            var userIdValue = UserClaimsPrincipal
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdValue))
            {
                return View(new List<NotificationViewModel>());
            }

            var userId = int.Parse(userIdValue);

            var notifications = await _notificationService
                .GetUserNotificationsAsync(userId);

            ViewBag.UnreadCount = notifications.Count(x => !x.IsRead);

            return View(notifications.Take(5).ToList());
        }
    }
}
