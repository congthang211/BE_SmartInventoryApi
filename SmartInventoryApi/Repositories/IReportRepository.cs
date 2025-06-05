using SmartInventoryApi.DTOs; // Cần thiết cho các DTOs query parameters
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IReportRepository
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? warehouseId); // warehouseId để lọc theo kho cụ thể nếu cần
        Task<IEnumerable<Inventory>> GetInventoryReportDataAsync(InventoryReportQueryParameters queryParameters);
        Task<int> GetInventoryReportTotalCountAsync(InventoryReportQueryParameters queryParameters);
        Task<IEnumerable<InventoryTransaction>> GetInventoryTransactionReportDataAsync(InventoryTransactionReportQueryParameters queryParameters);
        Task<int> GetInventoryTransactionReportTotalCountAsync(InventoryTransactionReportQueryParameters queryParameters);
    }
}
