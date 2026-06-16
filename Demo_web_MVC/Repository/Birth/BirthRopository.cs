using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Birth
{
    public class BirthRopository : IBirthRopository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<BirthRopository> _logger;
        public BirthRopository (AppDatabase database, ILogger<BirthRopository> logger)
        {
            _context = database;
            _logger = logger;
        }
        public async Task<List<User>> GetUsersHaveBirthdayToday()
        {
            var today = DateTime.Today;

            return await _context.Users
                .Where(x =>
                    x.DateOfBirth != null &&
                    x.DateOfBirth.Value.Day == today.Day &&
                    x.DateOfBirth.Value.Month == today.Month &&
                    (x.LastBirthdayEmailYear == null ||
                     x.LastBirthdayEmailYear != today.Year) &&

                    x.UserRoles.Any(ur => ur.Role.Name == "USER"))
                .ToListAsync();
        }
        public async Task UpdateLastBirthdayEmailYear(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return;
            }

            user.LastBirthdayEmailYear = DateTime.Now.Year;
            await _context.SaveChangesAsync();
        }
        public async Task UpdateVipUsersAsync()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    User = u,

                    TotalProducts = u.Orders
                        .Where(o => o.Status == OrderStatus.Completed)
                        .SelectMany(o => o.OrderItems)
                        .Sum(i => (int?)i.Quantity) ?? 0,

                    TotalSpent = u.Orders
                        .Where(o => o.Status == OrderStatus.Completed)
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToListAsync();

            foreach (var item in users)
            {
                item.User.IsVip =
                    item.TotalProducts >= 5 ||
                    item.TotalSpent >= 50000000;
            }

            await _context.SaveChangesAsync();
        }
    }
}
