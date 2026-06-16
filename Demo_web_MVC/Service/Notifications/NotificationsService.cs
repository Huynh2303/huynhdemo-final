using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Repository.Notifications;

namespace Demo_web_MVC.Service.Notifications
{
    public class NotificationsService : INotificationsService
    {
        private readonly INotificationsRepository _notificationRepository;

        public NotificationsService(INotificationsRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task CreateAsync(
            int userId,
            string title,
            string content,
            string type,
            int? referenceId = null,
            string? url = null)
        {
            var notification = new Models.Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceId = referenceId,
                Url = url,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            return notifications.Select(x => new NotificationViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                Type = x.Type,
                Url = x.Url,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.CountUnreadAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }
        public async Task<NotificationViewModel?> GetByIdAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository
                .GetByIdAsync(notificationId, userId);

            if (notification == null)
                return null;

            return new NotificationViewModel
            {
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                Type = notification.Type,
                Url = notification.Url,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
