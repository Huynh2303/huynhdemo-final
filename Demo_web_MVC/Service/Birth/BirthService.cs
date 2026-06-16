using Demo_web_MVC.Repository.Birth;

using Demo_web_MVC.Service.Sendemail;
namespace Demo_web_MVC.Service.Birth
{
    public class BirthService : IBirthService
    {
        private readonly IBirthRopository _birthRopository;
        private readonly ILogger<BirthService> _logger;
        private readonly IEmailServices _emailService;
        public BirthService(IBirthRopository birthRopository,ILogger<BirthService> logger, IEmailServices emailService)
        {
            _logger = logger;
            _birthRopository = birthRopository;
            _emailService = emailService;
        }
        public async Task SendBirthdayEmailsAsync()
        {
            _logger.LogInformation("Bắt đầu kiểm tra sinh nhật ngày {Date}",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            var users = await _birthRopository.GetUsersHaveBirthdayToday();

            _logger.LogInformation("Tìm thấy {Count} khách hàng có sinh nhật hôm nay",
                users.Count);

            foreach (var user in users)
            {
                try
                {
                    _logger.LogInformation(
                        "Đang gửi email sinh nhật cho UserId={UserId}, Email={Email}",
                        user.Id,
                        user.Email);

                    await _emailService.SendEmailAsync(
                        user.Email,
                        "🎂 Chúc mừng sinh nhật",
                        $"Chúc mừng sinh nhật {user.FullName}! Chúc bạn có một ngày thật vui vẻ."
                    );

                    await _birthRopository.UpdateLastBirthdayEmailYear(user.Id);

                    _logger.LogInformation(
                        "Đã gửi email sinh nhật thành công cho UserId={UserId}",
                        user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Gửi email sinh nhật thất bại cho UserId={UserId}",
                        user.Id);
                }
            }

            _logger.LogInformation("Kết thúc job gửi email sinh nhật");
        }
        public async Task UpdateVipUsersAsync()
        {
            await _birthRopository.UpdateVipUsersAsync();
        }
    }
}
