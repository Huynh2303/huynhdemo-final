using Demo_web_MVC.Service.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationsService _notificationService;

        public NotificationsController(INotificationsService notificationService)
        {
            _notificationService = notificationService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var notifications = await _notificationService
                .GetUserNotificationsAsync(userId);

            return View(notifications);
        }

       

        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetUserId();

            var count = await _notificationService
                .GetUnreadCountAsync(userId);

            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();

            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok();
        }

   
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            await _notificationService.MarkAllAsReadAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdValue))
            {
                throw new UnauthorizedAccessException("User chưa đăng nhập.");
            }

            return int.Parse(userIdValue);
        }
        public async Task<IActionResult> Open(int id)
        {
            var userId = GetUserId();

            var notification = await _notificationService.GetByIdAsync(id, userId);

            if (notification == null)
            {
                return NotFound();
            }

            await _notificationService.MarkAsReadAsync(id, userId);

            if (string.IsNullOrEmpty(notification.Url))
            {
                return RedirectToAction(nameof(Index));
            }

            return Redirect(notification.Url);
        }
    }
}
