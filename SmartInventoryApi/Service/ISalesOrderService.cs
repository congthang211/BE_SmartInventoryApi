using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface ISalesOrderService
    {
        Task<PaginatedResponseDto<SalesOrderDto>> GetAllSalesOrdersAsync(
            SalesOrderQueryParameters queryParameters,
            int requestingUserId,
            string requestingUserRole);

        Task<SalesOrderDto?> GetSalesOrderByIdAsync(
            int id,
            int requestingUserId,
            string requestingUserRole);

        Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto salesOrderDto, int createdByUserId);
        Task<SalesOrderDto> UpdateSalesOrderAsync(int id, UpdateSalesOrderDto salesOrderDto, int updatedByUserId);
        Task<SalesOrderDto> ApproveSalesOrderAsync(int id, int approvedByUserId); // Approve and process inventory
        Task<SalesOrderDto> CancelSalesOrderAsync(int id, int cancelledByUserId);
    }
}