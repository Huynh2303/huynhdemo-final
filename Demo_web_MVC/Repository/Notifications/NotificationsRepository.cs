using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Demo_web_MVC.Repository.Notifications
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly AppDatabase _context;

        public NotificationsRepository(AppDatabase context)
        {
            _context = context;
        }
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    !x.IsRead);
        }

        public async Task MarkAsReadAsync(
            int notificationId,
            int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

            if (notification == null)
                return;

            notification.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return;

            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<Notification?> GetByIdAsync(int notificationId, int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);
        }
    }
}
