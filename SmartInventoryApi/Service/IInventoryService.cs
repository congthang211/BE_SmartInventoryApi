using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IInventoryService
    {
        Task<ProductStockDto?> GetProductStockAsync(int productId, int warehouseId);
    }
}
