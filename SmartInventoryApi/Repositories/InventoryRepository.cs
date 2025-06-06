using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryManagementDbContext _context;

        public InventoryRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetProductStockInWarehouseAsync(int productId, int warehouseId)
        {
            return await _context.Inventories
                                 .Include(i => i.Product)
                                 .Include(i => i.Warehouse)
                                 .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
        }
    }
}
