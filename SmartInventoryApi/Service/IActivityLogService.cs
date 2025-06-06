using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IActivityLogService
    {
        Task<PaginatedResponseDto<ActivityLogDto>> GetActivityLogsAsync(
            ActivityLogQueryParameters queryParameters,
            int requestingUserId,      // ID của người dùng đang thực hiện yêu cầu
            string requestingUserRole  // Vai trò của người dùng đang thực hiện yêu cầu
        );
    }
}
