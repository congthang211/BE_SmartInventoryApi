using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly InventoryManagementDbContext _context;

        public ActivityLogRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(ActivityLogQueryParameters queryParameters)
        {
            var query = _context.ActivityLogs.Include(log => log.User).AsQueryable();

            query = ApplyFilters(query, queryParameters);


            // Sắp xếp
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                var descending = queryParameters.SortDirection?.ToLower() == "desc";
                // Sắp xếp đơn giản, có thể mở rộng nếu cần nhiều cột hơn
                switch (queryParameters.SortBy.ToLower())
                {
                    case "logdate":
                        query = descending ? query.OrderByDescending(l => l.LogDate) : query.OrderBy(l => l.LogDate);
                        break;
                    case "username":
                        query = descending ? query.OrderByDescending(l => l.User.Username) : query.OrderBy(l => l.User.Username);
                        break;
                    case "module":
                        query = descending ? query.OrderByDescending(l => l.Module) : query.OrderBy(l => l.Module);
                        break;
                    default:
                        query = query.OrderByDescending(l => l.LogDate); // Mặc định
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(l => l.LogDate); // Mặc định
            }

            // Phân trang
            query = query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                         .Take(queryParameters.PageSize);

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalLogsCountAsync(ActivityLogQueryParameters queryParameters)
        {
            var query = _context.ActivityLogs.Include(log => log.User).AsQueryable();
            query = ApplyFilters(query, queryParameters);
            return await query.CountAsync();
        }

        private IQueryable<ActivityLog> ApplyFilters(IQueryable<ActivityLog> query, ActivityLogQueryParameters queryParameters)
        {
            if (queryParameters.UserId.HasValue)
            {
                query = query.Where(log => log.UserId == queryParameters.UserId.Value);
            }
            if (queryParameters.TargetUserRoles != null && queryParameters.TargetUserRoles.Any())
            {
                // Đảm bảo User không null trước khi truy cập UserRole
                query = query.Where(log => log.User != null && queryParameters.TargetUserRoles.Contains(log.User.UserRole));
            }
            if (!string.IsNullOrEmpty(queryParameters.Module))
            {
                query = query.Where(log => log.Module.Contains(queryParameters.Module));
            }
            if (!string.IsNullOrEmpty(queryParameters.Action))
            {
                query = query.Where(log => log.Action.Contains(queryParameters.Action));
            }
            if (!string.IsNullOrEmpty(queryParameters.EntityType))
            {
                query = query.Where(log => log.EntityType != null && log.EntityType.Contains(queryParameters.EntityType));
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(log => log.LogDate >= queryParameters.StartDate.Value);
            }
            if (queryParameters.EndDate.HasValue)
            {
                // Thêm 1 ngày và trừ 1 tick để bao gồm cả ngày EndDate
                var endDateInclusive = queryParameters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(log => log.LogDate <= endDateInclusive);
            }
            return query;
        }
    }
}