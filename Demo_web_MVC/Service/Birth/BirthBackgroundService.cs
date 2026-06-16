using Demo_web_MVC.Service.Birth;

namespace Demo_web_MVC.Service.Birth
{
    public class BirthBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BirthBackgroundService> _logger;

        public BirthBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BirthBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BirthBackgroundService đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Background đang chạy lúc {Time}",
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    using var scope = _scopeFactory.CreateScope();

                    var birthService = scope.ServiceProvider
                        .GetRequiredService<IBirthService>();

                    // Gửi email sinh nhật
                    await birthService.SendBirthdayEmailsAsync();

                    // Cập nhật khách hàng VIP
                    await birthService.UpdateVipUsersAsync();

                    _logger.LogInformation("Đã kiểm tra sinh nhật và cập nhật VIP.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong BirthBackgroundService.");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}