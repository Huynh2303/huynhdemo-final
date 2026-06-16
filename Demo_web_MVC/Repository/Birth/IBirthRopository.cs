using Demo_web_MVC.Models;

namespace Demo_web_MVC.Repository.Birth
{
    public interface IBirthRopository
    {
         Task<List<User>> GetUsersHaveBirthdayToday();
        Task UpdateLastBirthdayEmailYear(int userId);
        Task UpdateVipUsersAsync();
    }
}
