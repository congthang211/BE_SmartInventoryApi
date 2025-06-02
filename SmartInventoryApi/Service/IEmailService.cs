namespace SmartInventoryApi.Services
{
    public interface IEmailService
    {
        Task SendResetPasswordEmailAsync(string email, string resetToken, string userName);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
