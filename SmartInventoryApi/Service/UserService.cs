using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;
using System; // For Console.WriteLine
using System.Threading.Tasks; // For Task
using Microsoft.Extensions.Logging; // For ILogger (optional, but good practice)

namespace SmartInventoryApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger; // Optional: for more structured logging

        // Đảm bảo constructor của bạn đã bao gồm ITokenService và ILogger (nếu dùng)
        public UserService(
            IUserRepository userRepository,
            IEmailService emailService,
            ITokenService tokenService,
            ILogger<UserService> logger) // Thêm ILogger nếu bạn muốn dùng logging chuẩn
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger; // Gán logger
        }

        // ... các phương thức khác không đổi ...
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return MapToResponseDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            if (await _userRepository.UsernameExistsAsync(createUserDto.Username))
                throw new InvalidOperationException("Username already exists");

            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Username = createUserDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                UserRole = createUserDto.UserRole,
                IsActive = true,
                CreatedDate = DateTime.UtcNow // Nên đặt CreatedDate ở đây
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToResponseDto(createdUser);
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrEmpty(updateUserDto.Email) &&
                updateUserDto.Email != user.Email &&
                await _userRepository.EmailExistsAsync(updateUserDto.Email, id))
            {
                throw new InvalidOperationException("Email already exists");
            }

            if (!string.IsNullOrEmpty(updateUserDto.FullName))
                user.FullName = updateUserDto.FullName;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.UserRole))
                user.UserRole = updateUserDto.UserRole;

            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToResponseDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                throw new KeyNotFoundException("User not found");
            return MapToResponseDto(user);
        }


        public async Task<bool> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("[ForgotPasswordAsync] Starting for email: {Email}", email);
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("[ForgotPasswordAsync] User not found or inactive for email: {Email}. Proceeding silently.", email);
                // Không tiết lộ email có tồn tại hay không, vẫn trả về true
                return true;
            }

            _logger.LogInformation("[ForgotPasswordAsync] User found: {UserId}. Generating OTP.", user.Id);
            var otp = _tokenService.GenerateResetToken(email); // Đã đổi tên từ resetToken thành otp
            _logger.LogInformation("[ForgotPasswordAsync] OTP generated for email: {Email}. OTP: {OTP}", email, otp);


            await _emailService.SendResetPasswordEmailAsync(user.Email, otp, user.FullName);
            _logger.LogInformation("[ForgotPasswordAsync] Reset password email sent to: {Email}", email);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            _logger.LogInformation("[ResetPasswordAsync] Attempting to reset password for email: {Email} with OTP: {OTP}", email, otp);

            if (_tokenService == null)
            {
                _logger.LogError("[ResetPasswordAsync] ITokenService is NULL. Please check dependency injection.");
                return false;
            }

            bool isOtpValid = _tokenService.ValidateResetToken(email, otp);
            _logger.LogInformation("[ResetPasswordAsync] OTP validation result for email {Email}: {IsValid}", email, isOtpValid);

            if (!isOtpValid)
            {
                _logger.LogWarning("[ResetPasswordAsync] Invalid or expired OTP for email: {Email}", email);
                return false;
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("[ResetPasswordAsync] User not found for email: {Email} after OTP validation. This should not happen if OTP was valid for this email.", email);
                return false; // User không tồn tại (hoặc không active)
            }
            _logger.LogInformation("[ResetPasswordAsync] User found: {UserId}. Current password hash (for reference): {PasswordHash}", user.Id, user.Password.Substring(0, Math.Min(user.Password.Length, 10)) + "...");


            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _logger.LogInformation("[ResetPasswordAsync] New password hashed for user: {UserId}. New hash (start): {NewPasswordHash}", user.Id, user.Password.Substring(0, Math.Min(user.Password.Length, 10)) + "...");

            _logger.LogInformation("[ResetPasswordAsync] Calling repository to update user: {UserId}", user.Id);
            var updateResult = await _userRepository.UpdateAsync(user); // UpdateAsync trả về User, không phải bool

            if (updateResult != null)
            {
                _logger.LogInformation("[ResetPasswordAsync] User update successful for user: {UserId}", user.Id);
                _tokenService.RemoveResetToken(email);
                _logger.LogInformation("[ResetPasswordAsync] OTP removed for email: {Email}", email);
                return true;
            }
            else
            {
                _logger.LogError("[ResetPasswordAsync] User update FAILED for user: {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token) // token ở đây là OTP
        {
            _logger.LogInformation("[ValidateResetTokenAsync] Validating OTP for email: {Email}", email);
            if (_tokenService == null)
            {
                _logger.LogError("[ValidateResetTokenAsync] ITokenService is NULL.");
                return false;
            }
            return _tokenService.ValidateResetToken(email, token);
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                UserRole = user.UserRole,
                CreatedAt = user.CreatedDate, // Giả sử User model có CreatedDate
                IsActive = user.IsActive
            };
        }
    }
}
