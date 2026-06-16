namespace Demo_web_MVC.Service.Sendemail
{
    public interface IEmailServices
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
