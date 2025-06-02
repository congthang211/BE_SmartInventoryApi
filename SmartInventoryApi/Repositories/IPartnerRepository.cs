using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<Partner>> GetAllAsync(string? type);
        Task<Partner?> GetByIdAsync(int id);
        Task<Partner> CreateAsync(Partner partner);
        Task UpdateAsync(Partner partner);
        Task DeleteAsync(Partner partner);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}