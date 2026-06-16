using Demo_web_MVC.Models;

namespace Demo_web_MVC.Repository.Notifications
{
    public interface INotificationsRepository
    {
        Task AddAsync(Notification notification);

        Task<List<Notification>> GetByUserIdAsync(int userId);

        Task<int> CountUnreadAsync(int userId);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task MarkAllAsReadAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId, int userId);
    }
}
