using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly InventoryManagementDbContext _context;

        public SalesOrderRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            // OrderType được set ở Service Layer
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllAsync(SalesOrderQueryParameters queryParameters)
        {
            var query = _context.Orders
                                .Include(o => o.Partner)    // Khách hàng
                                .Include(o => o.Warehouse)  // Kho xuất
                                .Include(o => o.CreatedByNavigation) // Người tạo
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Product) // Sản phẩm trong chi tiết
                                .Where(o => o.OrderType == "Sale")
                                .AsQueryable();

            query = ApplyFilters(query, queryParameters);
            query = ApplySorting(query, queryParameters);

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(SalesOrderQueryParameters queryParameters)
        {
            var query = _context.Orders.Where(o => o.OrderType == "Sale").AsQueryable();
            query = ApplyFilters(query, queryParameters);
            return await query.CountAsync();
        }

        private IQueryable<Order> ApplyFilters(IQueryable<Order> query, SalesOrderQueryParameters queryParameters)
        {
            if (queryParameters.PartnerId.HasValue)
            {
                query = query.Where(o => o.PartnerId == queryParameters.PartnerId.Value);
            }
            if (queryParameters.WarehouseId.HasValue)
            {
                query = query.Where(o => o.WarehouseId == queryParameters.WarehouseId.Value);
            }
            if (!string.IsNullOrEmpty(queryParameters.Status))
            {
                query = query.Where(o => o.Status == queryParameters.Status);
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= queryParameters.StartDate.Value);
            }
            if (queryParameters.EndDate.HasValue)
            {
                var endDateInclusive = queryParameters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endDateInclusive);
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                query = query.Where(o => o.Code.Contains(queryParameters.SearchTerm));
            }
            return query;
        }

        private IQueryable<Order> ApplySorting(IQueryable<Order> query, SalesOrderQueryParameters queryParameters)
        {
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "partnername": // Customer Name
                    query = descending ? query.OrderByDescending(o => o.Partner.Name) : query.OrderBy(o => o.Partner.Name);
                    break;
                case "status":
                    query = descending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status);
                    break;
                case "totalamount":
                    query = descending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount);
                    break;
                case "orderdate":
                default:
                    query = descending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                    break;
            }
            return query;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                                 .Include(o => o.Partner)
                                 .Include(o => o.Warehouse)
                                 .Include(o => o.CreatedByNavigation)
                                 .Include(o => o.OrderDetails)
                                     .ThenInclude(od => od.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id && o.OrderType == "Sale");
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetNextSalesOrderCodeAsync()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"PXK-{datePart}-"; // PXK: Phiếu Xuất Kho

            var lastOrder = await _context.Orders
                                        .Where(o => o.OrderType == "Sale" && o.Code.StartsWith(prefix))
                                        .OrderByDescending(o => o.Code)
                                        .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var lastNumberStr = lastOrder.Code.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"{prefix}{nextNumber:D4}";
        }
    }
}
