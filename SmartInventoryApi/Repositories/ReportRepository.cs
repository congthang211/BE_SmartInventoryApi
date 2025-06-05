using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;

namespace SmartInventoryApi.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly InventoryManagementDbContext _context;

        public ReportRepository(InventoryManagementDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? warehouseId)
        {
            var summary = new DashboardSummaryDto();

            var productQuery = _context.Products.Where(p => p.IsActive);
            var inventoryQuery = _context.Inventories.AsQueryable();
            var purchaseOrderQuery = _context.Orders.Where(o => o.OrderType == "Purchase");
            var salesOrderQuery = _context.Orders.Where(o => o.OrderType == "Sale");

            if (warehouseId.HasValue) // Nếu có warehouseId, lọc theo kho
            {
                inventoryQuery = inventoryQuery.Where(i => i.WarehouseId == warehouseId.Value);
                // Đối với đơn hàng, việc lọc theo kho có thể phức tạp hơn nếu 1 đơn hàng có thể liên quan nhiều kho
                // Tạm thời, nếu dashboard có kho, các đơn hàng cũng nên lọc theo kho đó
                purchaseOrderQuery = purchaseOrderQuery.Where(o => o.WarehouseId == warehouseId.Value);
                salesOrderQuery = salesOrderQuery.Where(o => o.WarehouseId == warehouseId.Value);
            }

            summary.TotalProducts = await productQuery.CountAsync();

            summary.TotalInventoryValue = await inventoryQuery
                .Include(i => i.Product)
                .SumAsync(i => i.Quantity * i.Product.CostPrice);

            summary.PendingPurchaseOrders = await purchaseOrderQuery
                .CountAsync(o => o.Status == "Pending" || o.Status == "Approved"); // Chờ xử lý hoặc đã duyệt nhưng chưa hoàn thành

            summary.PendingSalesOrders = await salesOrderQuery
                .CountAsync(o => o.Status == "Pending" || o.Status == "Approved");

            summary.LowStockProducts = await inventoryQuery
                .Include(i => i.Product)
                .CountAsync(i => i.Product.IsActive && i.Quantity < i.Product.MinimumStock);

            // summary.ProductsNearExpiry = 0; // Cần logic quản lý hạn sử dụng để tính

            return summary;
        }

        public async Task<IEnumerable<Inventory>> GetInventoryReportDataAsync(InventoryReportQueryParameters queryParameters)
        {
            var query = _context.Inventories
                                .Include(i => i.Product)
                                    .ThenInclude(p => p.Category)
                                .Include(i => i.Warehouse)
                                .Where(i => i.Product.IsActive) // Chỉ lấy sản phẩm active
                                .AsQueryable();

            if (queryParameters.WarehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == queryParameters.WarehouseId.Value);
            }
            if (queryParameters.CategoryId.HasValue)
            {
                query = query.Where(i => i.Product.CategoryId == queryParameters.CategoryId.Value);
            }
            if (queryParameters.OnlyLowStock == true)
            {
                query = query.Where(i => i.Quantity < i.Product.MinimumStock);
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                var term = queryParameters.SearchTerm.ToLower();
                query = query.Where(i => i.Product.Name.ToLower().Contains(term) || i.Product.Code.ToLower().Contains(term));
            }

            // Sorting
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "productcode":
                    query = descending ? query.OrderByDescending(i => i.Product.Code) : query.OrderBy(i => i.Product.Code);
                    break;
                case "categoryname":
                    query = descending ? query.OrderByDescending(i => i.Product.Category.Name) : query.OrderBy(i => i.Product.Category.Name);
                    break;
                case "warehousename":
                    query = descending ? query.OrderByDescending(i => i.Warehouse.Name) : query.OrderBy(i => i.Warehouse.Name);
                    break;
                case "currentquantity":
                    query = descending ? query.OrderByDescending(i => i.Quantity) : query.OrderBy(i => i.Quantity);
                    break;
                case "totalcostvalue":
                    query = descending ? query.OrderByDescending(i => i.Quantity * i.Product.CostPrice) : query.OrderBy(i => i.Quantity * i.Product.CostPrice);
                    break;
                case "productname":
                default:
                    query = descending ? query.OrderByDescending(i => i.Product.Name) : query.OrderBy(i => i.Product.Name);
                    break;
            }

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetInventoryReportTotalCountAsync(InventoryReportQueryParameters queryParameters)
        {
            var query = _context.Inventories
                                .Include(i => i.Product)
                                .Where(i => i.Product.IsActive)
                                .AsQueryable();
            if (queryParameters.WarehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == queryParameters.WarehouseId.Value);
            }
            if (queryParameters.CategoryId.HasValue)
            {
                query = query.Where(i => i.Product.CategoryId == queryParameters.CategoryId.Value);
            }
            if (queryParameters.OnlyLowStock == true)
            {
                query = query.Where(i => i.Quantity < i.Product.MinimumStock);
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                var term = queryParameters.SearchTerm.ToLower();
                query = query.Where(i => i.Product.Name.ToLower().Contains(term) || i.Product.Code.ToLower().Contains(term));
            }
            return await query.CountAsync();
        }


        public async Task<IEnumerable<InventoryTransaction>> GetInventoryTransactionReportDataAsync(InventoryTransactionReportQueryParameters queryParameters)
        {
            var query = _context.InventoryTransactions
                                .Include(t => t.Product)
                                .Include(t => t.SourceWarehouse)
                                .Include(t => t.DestinationWarehouse)
                                .Include(t => t.CreatedByNavigation) // User thực hiện
                                .AsQueryable();

            if (queryParameters.WarehouseId.HasValue) // Lọc theo kho nguồn hoặc kho đích
            {
                query = query.Where(t => t.SourceWarehouseId == queryParameters.WarehouseId.Value || t.DestinationWarehouseId == queryParameters.WarehouseId.Value);
            }
            if (queryParameters.ProductId.HasValue)
            {
                query = query.Where(t => t.ProductId == queryParameters.ProductId.Value);
            }
            if (!string.IsNullOrEmpty(queryParameters.TransactionType))
            {
                query = query.Where(t => t.TransactionType == queryParameters.TransactionType);
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= queryParameters.StartDate.Value);
            }
            if (queryParameters.EndDate.HasValue)
            {
                var endDateInclusive = queryParameters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionDate <= endDateInclusive);
            }
            if (queryParameters.UserId.HasValue)
            {
                query = query.Where(t => t.CreatedBy == queryParameters.UserId.Value);
            }

            // Sorting
            var descending = queryParameters.SortDirection?.ToLower() == "desc";
            switch (queryParameters.SortBy?.ToLower())
            {
                case "productname":
                    query = descending ? query.OrderByDescending(t => t.Product.Name) : query.OrderBy(t => t.Product.Name);
                    break;
                case "transactiontype":
                    query = descending ? query.OrderByDescending(t => t.TransactionType) : query.OrderBy(t => t.TransactionType);
                    break;
                case "userfullname":
                    query = descending ? query.OrderByDescending(t => t.CreatedByNavigation.FullName) : query.OrderBy(t => t.CreatedByNavigation.FullName);
                    break;
                case "transactiondate":
                default:
                    query = descending ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate);
                    break;
            }

            return await query.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                              .Take(queryParameters.PageSize)
                              .ToListAsync();
        }

        public async Task<int> GetInventoryTransactionReportTotalCountAsync(InventoryTransactionReportQueryParameters queryParameters)
        {
            var query = _context.InventoryTransactions.AsQueryable();

            if (queryParameters.WarehouseId.HasValue)
            {
                query = query.Where(t => t.SourceWarehouseId == queryParameters.WarehouseId.Value || t.DestinationWarehouseId == queryParameters.WarehouseId.Value);
            }
            if (queryParameters.ProductId.HasValue)
            {
                query = query.Where(t => t.ProductId == queryParameters.ProductId.Value);
            }
            if (!string.IsNullOrEmpty(queryParameters.TransactionType))
            {
                query = query.Where(t => t.TransactionType == queryParameters.TransactionType);
            }
            if (queryParameters.StartDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= queryParameters.StartDate.Value);
            }
            if (queryParameters.EndDate.HasValue)
            {
                var endDateInclusive = queryParameters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionDate <= endDateInclusive);
            }
            if (queryParameters.UserId.HasValue)
            {
                query = query.Where(t => t.CreatedBy == queryParameters.UserId.Value);
            }
            return await query.CountAsync();
        }
    }
}
