using SmartInventoryApi.DTOs;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;

        public ActivityLogService(IActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }

        public async Task<PaginatedResponseDto<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQueryParameters queryParameters)
        {
            var logs = await _activityLogRepository.GetLogsAsync(queryParameters);
            var totalCount = await _activityLogRepository.GetTotalLogsCountAsync(queryParameters);

            var logDtos = logs.Select(log => new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.User?.Username, // User có thể null nếu đã bị xóa cứng
                UserFullName = log.User?.FullName,
                Action = log.Action,
                Module = log.Module,
                Description = log.Description,
                EntityId = log.EntityId,
                EntityType = log.EntityType,
                IpAddress = log.Ipaddress,
                LogDate = log.LogDate
            });

            return new PaginatedResponseDto<ActivityLogDto>(
                logDtos,
                queryParameters.PageNumber,
                queryParameters.PageSize,
                totalCount
            );
        }
    }
}