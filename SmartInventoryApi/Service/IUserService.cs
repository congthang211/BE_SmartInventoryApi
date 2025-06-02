using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> GetUserByIdAsync(int id);
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
        Task<UserResponseDto> GetCurrentUserAsync(string username);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ValidateResetTokenAsync(string email, string token);
    }
}
