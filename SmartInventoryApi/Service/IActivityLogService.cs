using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IActivityLogService
    {
        Task<PaginatedResponseDto<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQueryParameters queryParameters);
    }
}