using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly InventoryManagementDbContext _context; // Để lấy mã phiếu liên quan cho transaction report

        public ReportService(IReportRepository reportRepository, InventoryManagementDbContext context)
        {
            _reportRepository = reportRepository;
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? warehouseId)
        {
            return await _reportRepository.GetDashboardSummaryAsync(warehouseId);
        }

        public async Task<PaginatedResponseDto<InventoryReportItemDto>> GetInventoryReportAsync(InventoryReportQueryParameters queryParameters)
        {
            var inventoryData = await _reportRepository.GetInventoryReportDataAsync(queryParameters);
            var totalCount = await _reportRepository.GetInventoryReportTotalCountAsync(queryParameters);

            var reportItems = inventoryData.Select(i => new InventoryReportItemDto
            {
                ProductId = i.ProductId,
                ProductCode = i.Product.Code,
                ProductName = i.Product.Name,
                ProductCategory = i.Product.Category?.Name ?? "N/A",
                Unit = i.Product.Unit,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                CurrentQuantity = i.Quantity,
                CostPrice = i.Product.CostPrice,
                TotalCostValue = i.Quantity * i.Product.CostPrice,
                MinimumStock = i.Product.MinimumStock
            }).ToList();

            return new PaginatedResponseDto<InventoryReportItemDto>(reportItems, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }

        public async Task<PaginatedResponseDto<InventoryTransactionReportItemDto>> GetInventoryTransactionReportAsync(InventoryTransactionReportQueryParameters queryParameters)
        {
            var transactionData = await _reportRepository.GetInventoryTransactionReportDataAsync(queryParameters);
            var totalCount = await _reportRepository.GetInventoryTransactionReportTotalCountAsync(queryParameters);

            var reportItems = new List<InventoryTransactionReportItemDto>();
            foreach (var t in transactionData)
            {
                string? referenceOrderCode = null;
                if (t.ReferenceId.HasValue)
                {
                    var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == t.ReferenceId.Value);
                    if (order != null)
                    {
                        referenceOrderCode = order.Code;
                    }
                    else // Có thể là StockTake
                    {
                        var stockTake = await _context.StockTakes.AsNoTracking().FirstOrDefaultAsync(st => st.Id == t.ReferenceId.Value);
                        referenceOrderCode = stockTake?.Code;
                    }
                }

                reportItems.Add(new InventoryTransactionReportItemDto
                {
                    TransactionId = t.Id,
                    TransactionCode = t.TransactionCode,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    ProductId = t.ProductId,
                    ProductCode = t.Product.Code,
                    ProductName = t.Product.Name,
                    QuantityChanged = (t.TransactionType == "Import" || t.TransactionType == "StockTakeAdjustment" && t.DestinationWarehouseId.HasValue) ? t.Quantity : -t.Quantity,
                    SourceWarehouseId = t.SourceWarehouseId,
                    SourceWarehouseName = t.SourceWarehouse?.Name,
                    DestinationWarehouseId = t.DestinationWarehouseId,
                    DestinationWarehouseName = t.DestinationWarehouse?.Name,
                    ReferenceOrderCode = referenceOrderCode,
                    UserId = t.CreatedBy,
                    UserFullName = t.CreatedByNavigation.FullName,
                    Notes = t.Notes
                });
            }

            return new PaginatedResponseDto<InventoryTransactionReportItemDto>(reportItems, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }
    }
}
