using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly InventoryManagementDbContext _context;

        public ActivityLogService(IActivityLogRepository activityLogRepository, InventoryManagementDbContext context)
        {
            _activityLogRepository = activityLogRepository;
            _context = context;
        }

        public async Task<PaginatedResponseDto<ActivityLogDto>> GetActivityLogsAsync(
            ActivityLogQueryParameters queryParameters,
            int requestingUserId,
            string requestingUserRole)
        {
            var effectiveQueryParameters = new ActivityLogQueryParameters
            {
                UserId = queryParameters.UserId,
                Module = queryParameters.Module,
                Action = queryParameters.Action,
                EntityType = queryParameters.EntityType,
                StartDate = queryParameters.StartDate,
                EndDate = queryParameters.EndDate,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                SortBy = queryParameters.SortBy,
                SortDirection = queryParameters.SortDirection,
                TargetUserRoles = queryParameters.TargetUserRoles != null ? new List<string>(queryParameters.TargetUserRoles) : new List<string>()
            };

            if (requestingUserRole == "Manager")
            {
                if (effectiveQueryParameters.UserId.HasValue)
                {
                    if (effectiveQueryParameters.UserId.Value == requestingUserId)
                    {
                        effectiveQueryParameters.TargetUserRoles.Clear();
                    }
                    else
                    {
                        var targetUser = await _context.Users.AsNoTracking()
                                                     .FirstOrDefaultAsync(u => u.Id == effectiveQueryParameters.UserId.Value);

                        if (targetUser == null || targetUser.UserRole != "Staff")
                        {

                            return new PaginatedResponseDto<ActivityLogDto>(
                                new List<ActivityLogDto>(),
                                effectiveQueryParameters.PageNumber,
                                effectiveQueryParameters.PageSize,
                                0
                            );
                        }
                        effectiveQueryParameters.TargetUserRoles.Clear();
                    }
                }
                else
                {
                    if (!effectiveQueryParameters.TargetUserRoles.Any())
                    {
                        effectiveQueryParameters.TargetUserRoles.Add("Staff");
                    }
                    else
                    {

                        {
                            effectiveQueryParameters.TargetUserRoles = new List<string> { "Staff" };
                        }

                    }
                }
            }


            var logs = await _activityLogRepository.GetLogsAsync(effectiveQueryParameters);
            var totalCount = await _activityLogRepository.GetTotalLogsCountAsync(effectiveQueryParameters);

            var logDtos = logs.Select(log => new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.User?.Username,
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
                effectiveQueryParameters.PageNumber,
                effectiveQueryParameters.PageSize,
                totalCount
            );
        }
    }
}
