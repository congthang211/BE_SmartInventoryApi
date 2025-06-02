namespace SmartInventoryApi.Services
{
    public interface ITokenService
    {
        string GenerateResetToken(string email);
        bool ValidateResetToken(string email, string token);
        void RemoveResetToken(string email);
    }
}
