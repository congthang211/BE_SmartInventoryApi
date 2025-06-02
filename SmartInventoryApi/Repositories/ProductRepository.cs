using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryManagementDbContext _context;

        public ProductRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Product product)
        {
            product.IsActive = false; // Soft delete
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductQueryParameters queryParameters)
        {
            var query = _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.DefaultSupplier)
                                .Include(p => p.CreatedByNavigation) // User người tạo
                                .AsQueryable();

            query = ApplyFilters(query, queryParameters);
            query = ApplySorting(query, queryParameters);

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(ProductQueryParameters queryParameters)
        {
            var query = _context.Products.AsQueryable();
            query = ApplyFilters(query, queryParameters);
            return await query.CountAsync();
        }

        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductQueryParameters queryParameters)
        {
            if (queryParameters.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == queryParameters.IsActive.Value);
            }

            if (queryParameters.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == queryParameters.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                var searchTermLower = queryParameters.SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTermLower) || p.Code.ToLower().Contains(searchTermLower));
            }
            return query;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductQueryParameters queryParameters)
        {
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "code":
                    query = descending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code);
                    break;
                case "categoryname":
                    query = descending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name);
                    break;
                case "costprice":
                    query = descending ? query.OrderByDescending(p => p.CostPrice) : query.OrderBy(p => p.CostPrice);
                    break;
                case "sellingprice":
                    query = descending ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice);
                    break;
                case "createddate":
                    query = descending ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate);
                    break;
                case "name":
                default:
                    query = descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
            }
            return query;
        }


        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.DefaultSupplier)
                                 .Include(p => p.CreatedByNavigation)
                                 .FirstOrDefaultAsync(p => p.Id == id); // Lấy cả sp active và inactive để manager có thể xem/sửa
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Products.Where(p => p.Code == code);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync(); // Kiểm tra cả sp inactive để tránh trùng code khi active lại
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Products.Where(p => p.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}