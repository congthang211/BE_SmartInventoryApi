using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class PartnerRepository : IPartnerRepository
    {
        private readonly InventoryManagementDbContext _context;

        public PartnerRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Partner> CreateAsync(Partner partner)
        {
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            return partner;
        }

        public async Task DeleteAsync(Partner partner)
        {
            partner.IsActive = false;
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Partner>> GetAllAsync(string? type)
        {
            var query = _context.Partners.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type == type);
            }

            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Partner?> GetByIdAsync(int id)
        {
            return await _context.Partners.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Partners.Where(p => p.Name == name && p.IsActive);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task UpdateAsync(Partner partner)
        {
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();
        }
    }
}