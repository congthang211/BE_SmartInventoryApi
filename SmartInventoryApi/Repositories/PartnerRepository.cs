using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
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
        public async Task<IEnumerable<Order>> GetOrdersByPartnerIdAsync(int partnerId, PartnerOrderHistoryQueryParameters queryParameters)
        {
            var query = _context.Orders
                                .Include(o => o.Warehouse) // Để lấy tên kho
                                .Where(o => o.PartnerId == partnerId)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(queryParameters.OrderType))
            {
                query = query.Where(o => o.OrderType == queryParameters.OrderType);
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date >= queryParameters.StartDate.Value.Date);
            }
            if (queryParameters.EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date <= queryParameters.EndDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(queryParameters.Status))
            {
                query = query.Where(o => o.Status == queryParameters.Status);
            }

            // Sorting
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "totalamount":
                    query = descending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount);
                    break;
                case "status":
                    query = descending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status);
                    break;
                case "orderdate":
                default:
                    query = descending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                    break;
            }

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetOrdersByPartnerIdCountAsync(int partnerId, PartnerOrderHistoryQueryParameters queryParameters)
        {
            var query = _context.Orders
                                .Where(o => o.PartnerId == partnerId)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(queryParameters.OrderType))
            {
                query = query.Where(o => o.OrderType == queryParameters.OrderType);
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date >= queryParameters.StartDate.Value.Date);
            }
            if (queryParameters.EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date <= queryParameters.EndDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(queryParameters.Status))
            {
                query = query.Where(o => o.Status == queryParameters.Status);
            }

            return await query.CountAsync();
        }
    }
}