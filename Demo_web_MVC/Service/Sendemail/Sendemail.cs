using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Demo_web_MVC.Service.Sendemail
{
   public class Sendemail : IEmailServices
    {
        private readonly IConfiguration _config;
        private readonly ILogger<Sendemail> _logger;

        public Sendemail(IConfiguration config, ILogger<Sendemail> logger)
        {
            _config = config;
            _logger = logger;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _config["MailSettings:DisplayName"],
                _config["MailSettings:Mail"]
            ));

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject ?? "";

            email.Body = new TextPart("html")
            {
                Text = body ?? ""
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["MailSettings:Host"],
                int.Parse(_config["MailSettings:Port"]),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["MailSettings:Mail"],
                _config["MailSettings:Password"]
            );
            bool send = true;
            
            try
            {
                await smtp.SendAsync(email);
                Directory.CreateDirectory("mailsave");
                var emailsavefile = string.Format(@"mailsave/{0}.eml", Guid.NewGuid());
                await email.WriteToAsync(emailsavefile);
                _logger.LogInformation("Lưu email vào file: " + emailsavefile);
            }
            catch (Exception ex)
            {
                send = false;
                Directory.CreateDirectory("mailsave");
                var emailsavefile = string.Format(@"mailsave/{0}.eml", Guid.NewGuid());
                await email.WriteToAsync(emailsavefile);
                _logger.LogInformation("Lưu email vào file: " + emailsavefile);
                _logger.LogError(ex.Message);
            }
            await smtp.DisconnectAsync(true);
            if(send)
            {
                 _logger.LogInformation("Đã gửi email đến: " + to);
            }
          
        }

    }
}
