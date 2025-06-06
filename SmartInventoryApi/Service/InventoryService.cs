using SmartInventoryApi.DTOs;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<ProductStockDto?> GetProductStockAsync(int productId, int warehouseId)
        {
            var inventoryItem = await _inventoryRepository.GetProductStockInWarehouseAsync(productId, warehouseId);

            if (inventoryItem == null)
            {

                return null;
            }


            if (inventoryItem.Product == null || inventoryItem.Warehouse == null)
            {

                return null;
            }

            return new ProductStockDto
            {
                ProductId = inventoryItem.ProductId,
                ProductName = inventoryItem.Product.Name,
                ProductCode = inventoryItem.Product.Code,
                WarehouseId = inventoryItem.WarehouseId,
                WarehouseName = inventoryItem.Warehouse.Name,
                CurrentQuantity = inventoryItem.Quantity,
                Unit = inventoryItem.Product.Unit,
                MinimumStock = inventoryItem.Product.MinimumStock
            };
        }
    }
}
