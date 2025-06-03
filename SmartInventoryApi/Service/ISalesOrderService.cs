using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface ISalesOrderService
    {
        Task<PaginatedResponseDto<SalesOrderDto>> GetAllSalesOrdersAsync(SalesOrderQueryParameters queryParameters);
        Task<SalesOrderDto?> GetSalesOrderByIdAsync(int id);
        Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto salesOrderDto, int createdByUserId);
        Task<SalesOrderDto> UpdateSalesOrderAsync(int id, UpdateSalesOrderDto salesOrderDto, int updatedByUserId);
        Task<SalesOrderDto> ApproveSalesOrderAsync(int id, int approvedByUserId); // Approve and process inventory
        Task<SalesOrderDto> CancelSalesOrderAsync(int id, int cancelledByUserId);
    }
}
