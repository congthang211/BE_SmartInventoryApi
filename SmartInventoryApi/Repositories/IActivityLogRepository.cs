using SmartInventoryApi.DTOs; // Để sử dụng ActivityLogQueryParameters
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IActivityLogRepository
    {
        Task<IEnumerable<ActivityLog>> GetLogsAsync(ActivityLogQueryParameters queryParameters);
        Task<int> GetTotalLogsCountAsync(ActivityLogQueryParameters queryParameters);
    }
}