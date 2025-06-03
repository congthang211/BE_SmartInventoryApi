using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class StockTakeRepository : IStockTakeRepository
    {
        private readonly InventoryManagementDbContext _context;

        public StockTakeRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<StockTake> CreateAsync(StockTake stockTake)
        {
            _context.StockTakes.Add(stockTake);
            // StockTakeDetails được thêm vào context thông qua navigation property của stockTake
            await _context.SaveChangesAsync();
            return stockTake;
        }

        public async Task<IEnumerable<StockTake>> GetAllAsync(StockTakeQueryParameters queryParameters)
        {
            var query = _context.StockTakes
                                .Include(st => st.Warehouse)
                                .Include(st => st.CreatedByNavigation)
                                // Không include StockTakeDetails ở đây để tránh dữ liệu lớn,
                                // sẽ include khi lấy chi tiết một phiếu.
                                .AsQueryable();

            query = ApplyFilters(query, queryParameters);
            query = ApplySorting(query, queryParameters);

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(StockTakeQueryParameters queryParameters)
        {
            var query = _context.StockTakes.AsQueryable();
            query = ApplyFilters(query, queryParameters);
            return await query.CountAsync();
        }

        private IQueryable<StockTake> ApplyFilters(IQueryable<StockTake> query, StockTakeQueryParameters queryParameters)
        {
            if (queryParameters.WarehouseId.HasValue)
            {
                query = query.Where(st => st.WarehouseId == queryParameters.WarehouseId.Value);
            }
            if (!string.IsNullOrEmpty(queryParameters.Status))
            {
                query = query.Where(st => st.Status == queryParameters.Status);
            }
            if (queryParameters.StartDatePeriod.HasValue)
            {
                query = query.Where(st => st.StartDate.Date >= queryParameters.StartDatePeriod.Value.Date);
            }
            if (queryParameters.EndDatePeriod.HasValue)
            {
                query = query.Where(st => st.StartDate.Date <= queryParameters.EndDatePeriod.Value.Date);
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                query = query.Where(st => st.Code.Contains(queryParameters.SearchTerm));
            }
            return query;
        }

        private IQueryable<StockTake> ApplySorting(IQueryable<StockTake> query, StockTakeQueryParameters queryParameters)
        {
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "warehousename":
                    query = descending ? query.OrderByDescending(st => st.Warehouse.Name) : query.OrderBy(st => st.Warehouse.Name);
                    break;
                case "status":
                    query = descending ? query.OrderByDescending(st => st.Status) : query.OrderBy(st => st.Status);
                    break;
                case "startdate":
                default:
                    query = descending ? query.OrderByDescending(st => st.StartDate) : query.OrderBy(st => st.StartDate);
                    break;
            }
            return query;
        }


        public async Task<StockTake?> GetByIdAsync(int id)
        {
            return await _context.StockTakes
                                 .Include(st => st.Warehouse)
                                 .Include(st => st.CreatedByNavigation)
                                 .Include(st => st.StockTakeDetails)
                                     .ThenInclude(std => std.Product) // Lấy thông tin Product cho từng detail
                                 .FirstOrDefaultAsync(st => st.Id == id);
        }

        public async Task UpdateAsync(StockTake stockTake)
        {
            _context.StockTakes.Update(stockTake);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDetailAsync(StockTakeDetail detail)
        {
            _context.StockTakeDetails.Update(detail);
            // SaveChanges sẽ được gọi ở Service layer sau khi cập nhật nhiều details hoặc trong transaction
        }

        public async Task<string> GetNextStockTakeCodeAsync()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"PKK-{datePart}-"; // PKK: Phiếu Kiểm Kê

            var lastStockTake = await _context.StockTakes
                                        .Where(st => st.Code.StartsWith(prefix))
                                        .OrderByDescending(st => st.Code)
                                        .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastStockTake != null)
            {
                var lastNumberStr = lastStockTake.Code.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"{prefix}{nextNumber:D4}";
        }
    }
}
