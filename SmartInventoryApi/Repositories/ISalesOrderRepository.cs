using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync(SalesOrderQueryParameters queryParameters);
        Task<int> GetTotalCountAsync(SalesOrderQueryParameters queryParameters);
        Task<Order?> GetByIdAsync(int id); // Lấy cả OrderDetails
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task<string> GetNextSalesOrderCodeAsync();
    }
}
