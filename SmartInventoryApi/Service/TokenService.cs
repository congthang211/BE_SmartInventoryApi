using Microsoft.Extensions.Caching.Memory;
using System;

namespace SmartInventoryApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _tokenExpiry = TimeSpan.FromMinutes(5); // Giảm thời gian OTP hết hạn xuống 5 phút

        public TokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Thay đổi phương thức này để tạo OTP 6 số
        public string GenerateResetToken(string email)
        {
            // Tạo mã OTP 6 số ngẫu nhiên
            var otp = new Random().Next(100000, 999999).ToString();

            var cacheKey = $"reset_token_{email}";
            _cache.Set(cacheKey, otp, _tokenExpiry);

            return otp;
        }

        public bool ValidateResetToken(string email, string token)
        {
            var cacheKey = $"reset_token_{email}";
            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                return cachedToken == token;
            }
            return false;
        }

        public void RemoveResetToken(string email)
        {
            var cacheKey = $"reset_token_{email}";
            _cache.Remove(cacheKey);
        }
    }
}