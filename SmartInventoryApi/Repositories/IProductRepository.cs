using SmartInventoryApi.DTOs; // For ProductQueryParameters
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(ProductQueryParameters queryParameters);
        Task<int> GetTotalCountAsync(ProductQueryParameters queryParameters);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product); // Soft delete
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}