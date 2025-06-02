using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly InventoryManagementDbContext _context;

        public WarehouseRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task DeleteAsync(Warehouse warehouse)
        {
            // Soft delete
            warehouse.IsActive = false;
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id && w.IsActive);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Warehouses.Where(w => w.Name == name && w.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(w => w.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }
    }
}