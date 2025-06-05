using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? warehouseId);
        Task<PaginatedResponseDto<InventoryReportItemDto>> GetInventoryReportAsync(InventoryReportQueryParameters queryParameters);
        Task<PaginatedResponseDto<InventoryTransactionReportItemDto>> GetInventoryTransactionReportAsync(InventoryTransactionReportQueryParameters queryParameters);
    }
}
