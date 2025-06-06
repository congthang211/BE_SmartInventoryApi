using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetProductStockInWarehouseAsync(int productId, int warehouseId);

    }
}
