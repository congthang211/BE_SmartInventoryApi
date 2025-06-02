using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace SmartInventoryApi.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }

        public async Task SendResetPasswordEmailAsync(string email, string otp, string userName)
        {
            var subject = $"[{otp}] là mã khôi phục mật khẩu Smart Inventory của bạn";
            var body = GetOtpEmailTemplate(userName, otp);

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // ... phương thức này giữ nguyên, không thay đổi
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        // Tạo một mẫu email mới cho OTP
        private string GetOtpEmailTemplate(string userName, string otp)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Mã OTP đặt lại mật khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #667eea; text-align: center;'>Đặt lại mật khẩu</h2>
                        <p>Xin chào {userName},</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Smart Inventory của mình.</p>
                        <p>Vui lòng sử dụng mã OTP dưới đây để hoàn tất quá trình:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <p style='background-color: #f0f0f0; color: #333; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;
                                      font-size: 24px; letter-spacing: 5px; font-weight: bold;'>
                                {otp}
                            </p>
                        </div>
                        <p style='text-align: center;'>Mã này sẽ hết hạn sau 5 phút.</p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    </div>
                </body>
                </html>";
        }
    }
}
