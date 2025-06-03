using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync(PurchaseOrderQueryParameters queryParameters);
        Task<int> GetTotalCountAsync(PurchaseOrderQueryParameters queryParameters);
        Task<Order?> GetByIdAsync(int id); // Lấy cả OrderDetails
        Task<Order> CreateAsync(Order order); // order đã bao gồm OrderDetails
        Task UpdateAsync(Order order);
        Task<string> GetNextPurchaseOrderCodeAsync();
    }
}
