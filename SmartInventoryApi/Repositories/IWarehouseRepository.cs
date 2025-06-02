using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<Warehouse?> GetByIdAsync(int id);
        Task<Warehouse> CreateAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(Warehouse warehouse);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}