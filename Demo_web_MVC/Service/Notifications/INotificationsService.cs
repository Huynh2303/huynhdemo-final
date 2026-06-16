using Demo_web_MVC.Models.ViewModel;

namespace Demo_web_MVC.Service.Notifications
{
    public interface INotificationsService
    {
        Task CreateAsync(
            int userId,
            string title,
            string content,
            string type,
            int? referenceId = null,
            string? url = null);

        Task<List<NotificationViewModel>> GetUserNotificationsAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task MarkAllAsReadAsync(int userId);
        Task<NotificationViewModel?> GetByIdAsync(int notificationId, int userId);
    }
}
