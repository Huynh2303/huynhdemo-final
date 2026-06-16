namespace Demo_web_MVC.Service.Birth
{
    public interface IBirthService
    {
        Task SendBirthdayEmailsAsync();
        Task UpdateVipUsersAsync();
    }
}
