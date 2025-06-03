using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IStockTakeRepository
    {
        Task<IEnumerable<StockTake>> GetAllAsync(StockTakeQueryParameters queryParameters);
        Task<int> GetTotalCountAsync(StockTakeQueryParameters queryParameters);
        Task<StockTake?> GetByIdAsync(int id); // Lấy cả StockTakeDetails
        Task<StockTake> CreateAsync(StockTake stockTake); // stockTake đã bao gồm StockTakeDetails ban đầu
        Task UpdateAsync(StockTake stockTake); // Dùng để cập nhật status, notes, EndDate
        Task UpdateDetailAsync(StockTakeDetail detail); // Cập nhật một dòng chi tiết
        Task<string> GetNextStockTakeCodeAsync();
    }
}
