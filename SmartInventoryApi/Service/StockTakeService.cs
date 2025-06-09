using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class StockTakeService : IStockTakeService
    {
        private readonly IStockTakeRepository _stockTakeRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IProductRepository _productRepository; // Để lấy thông tin sản phẩm
        private readonly InventoryManagementDbContext _context; // Dùng cho transaction và truy vấn Inventory

        public StockTakeService(
            IStockTakeRepository stockTakeRepository,
            IWarehouseRepository warehouseRepository,
            IProductRepository productRepository,
            InventoryManagementDbContext context)
        {
            _stockTakeRepository = stockTakeRepository;
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<PaginatedResponseDto<StockTakeDto>> GetAllStockTakesAsync(
            StockTakeQueryParameters queryParameters,
            string requestingUserRole)
        {
            // Logic nghiệp vụ: Nếu người dùng là Staff, chỉ hiển thị các phiếu đang chờ thực hiện
            if (requestingUserRole == "Staff")
            {
                // Nếu client đã lọc theo status khác, ta sẽ ghi đè để đảm bảo Staff chỉ thấy việc của mình
                if (string.IsNullOrEmpty(queryParameters.Status) ||
                   (queryParameters.Status != "Draft" && queryParameters.Status != "InProgress"))
                {
                    // Mặc định Staff chỉ thấy các phiếu ở trạng thái "Draft".
                    // Bạn có thể thay đổi logic này để hiển thị cả "InProgress" nếu muốn.
                    queryParameters.Status = "Draft";
                }
            }

            var stockTakes = await _stockTakeRepository.GetAllAsync(queryParameters);
            var totalCount = await _stockTakeRepository.GetTotalCountAsync(queryParameters);
            var stockTakeDtos = stockTakes.Select(MapToDto);
            return new PaginatedResponseDto<StockTakeDto>(stockTakeDtos, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }

        public async Task<StockTakeDto?> GetStockTakeByIdAsync(int id, string requestingUserRole)
        {
            var stockTake = await _stockTakeRepository.GetByIdAsync(id);
            if (stockTake == null)
            {
                return null;
            }

            // Logic nghiệp vụ: Staff chỉ có thể xem chi tiết các phiếu đang chờ thực hiện
            if (requestingUserRole == "Staff")
            {
                if (stockTake.Status != "Draft" && stockTake.Status != "InProgress")
                {
                    // Không cho phép xem phiếu đã hoàn thành hoặc đã hủy
                    return null;
                }
            }

            return MapToDto(stockTake);
        }

        public async Task<StockTakeDto> CreateStockTakeAsync(CreateStockTakeDto dto, int createdByUserId)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }

            var stockTake = new StockTake
            {
                Code = await _stockTakeRepository.GetNextStockTakeCodeAsync(),
                WarehouseId = dto.WarehouseId,
                StartDate = dto.StartDate,
                Status = "Draft", // Trạng thái ban đầu
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedDate = DateTime.UtcNow,
                StockTakeDetails = new List<StockTakeDetail>()
            };

            List<Product> productsToTake;
            if (dto.ProductIdsToTake != null && dto.ProductIdsToTake.Any())
            {
                productsToTake = await _context.Products
                                        .Where(p => dto.ProductIdsToTake.Contains(p.Id) && p.IsActive)
                                        .ToListAsync();
            }
            else // Lấy tất cả sản phẩm có tồn kho hoặc tất cả sản phẩm active trong kho đó
            {
                // Lấy tất cả sản phẩm active, sau đó sẽ join với inventory để lấy số lượng
                productsToTake = await _context.Products.Where(p => p.IsActive).ToListAsync();
            }

            foreach (var product in productsToTake)
            {
                var inventoryItem = await _context.Inventories
                                        .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.WarehouseId == dto.WarehouseId);

                // Chỉ thêm vào phiếu kiểm kê nếu sản phẩm có trong danh sách chỉ định HOẶC
                // nếu không chỉ định sản phẩm và sản phẩm có tồn kho (inventoryItem != null)
                if ((dto.ProductIdsToTake != null && dto.ProductIdsToTake.Contains(product.Id)) ||
                    (dto.ProductIdsToTake == null && inventoryItem != null && inventoryItem.Quantity > 0) ||
                    (dto.ProductIdsToTake == null && inventoryItem == null)) // Trường hợp muốn kiểm kê cả sp chưa có tồn
                {
                    stockTake.StockTakeDetails.Add(new StockTakeDetail
                    {
                        ProductId = product.Id,
                        SystemQuantity = inventoryItem?.Quantity ?? 0, // Nếu chưa có tồn thì là 0
                        CountedQuantity = null, // Ban đầu chưa đếm
                        Variance = null
                    });
                }
            }

            if (!stockTake.StockTakeDetails.Any() && (dto.ProductIdsToTake != null && dto.ProductIdsToTake.Any()))
            {
                throw new InvalidOperationException("No specified active products found to include in the stock take.");
            }
            if (!stockTake.StockTakeDetails.Any() && (dto.ProductIdsToTake == null || !dto.ProductIdsToTake.Any()))
            {
                throw new InvalidOperationException("No products with current inventory (or no active products if taking all) found in the specified warehouse to initiate stock take.");
            }


            var createdStockTake = await _stockTakeRepository.CreateAsync(stockTake);
            // Tải lại để có đủ thông tin cho DTO
            var fullStockTake = await _stockTakeRepository.GetByIdAsync(createdStockTake.Id);
            return MapToDto(fullStockTake!);
        }

        public async Task<StockTakeDto> RecordStockTakeCountsAsync(int stockTakeId, RecordStockTakeCountsDto countsDto, int recordedByUserId)
        {
            var stockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
            if (stockTake == null)
            {
                throw new KeyNotFoundException("Stock take not found.");
            }
            if (stockTake.Status != "Draft" && stockTake.Status != "InProgress")
            {
                throw new InvalidOperationException($"Cannot record counts for stock take with status '{stockTake.Status}'.");
            }

            foreach (var itemDto in countsDto.Counts)
            {
                var detail = stockTake.StockTakeDetails.FirstOrDefault(d => d.ProductId == itemDto.ProductId);
                if (detail == null)
                {
                    // Hoặc throw exception, hoặc bỏ qua tùy nghiệp vụ
                    // throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found in this stock take.");
                    continue;
                }
                detail.CountedQuantity = itemDto.CountedQuantity;
                detail.Notes = itemDto.Notes; // Cập nhật ghi chú cho từng dòng
                detail.Variance = detail.SystemQuantity - itemDto.CountedQuantity; // Tính luôn variance
                await _stockTakeRepository.UpdateDetailAsync(detail);
            }

            stockTake.Status = "PendingApproval"; // Chuyển trạng thái chờ duyệt sau khi ghi nhận
            if (!string.IsNullOrEmpty(countsDto.OverallNotes))
            {
                stockTake.Notes = string.IsNullOrEmpty(stockTake.Notes) ? countsDto.OverallNotes : $"{stockTake.Notes}\nCounts Recorded Notes: {countsDto.OverallNotes}";
            }
            // Ghi nhận người thực hiện (nếu cần)
            await _stockTakeRepository.UpdateAsync(stockTake);
            await _context.SaveChangesAsync(); // Lưu các thay đổi cho StockTakeDetails

            var updatedStockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
            return MapToDto(updatedStockTake!);
        }

        public async Task<StockTakeDto> CompleteStockTakeAsync(int stockTakeId, int completedByUserId, string? completionNotes)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
                if (stockTake == null)
                {
                    throw new KeyNotFoundException("Stock take not found.");
                }
                if (stockTake.Status != "PendingApproval")
                {
                    throw new InvalidOperationException($"Only stock takes with status 'PendingApproval' can be completed. Current status: '{stockTake.Status}'.");
                }

                foreach (var detail in stockTake.StockTakeDetails)
                {
                    if (!detail.CountedQuantity.HasValue)
                    {
                        throw new InvalidOperationException($"Product '{detail.Product?.Name}' has not been counted yet.");
                    }

                    detail.Variance = detail.SystemQuantity - detail.CountedQuantity.Value;

                    if (detail.Variance != 0) // Chỉ xử lý khi có chênh lệch
                    {
                        // 1. Cập nhật Inventory
                        var inventoryItem = await _context.Inventories
                                                        .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId && i.WarehouseId == stockTake.WarehouseId);
                        if (inventoryItem == null && detail.CountedQuantity.Value > 0) // Nếu chưa có tồn mà đếm thấy có
                        {
                            inventoryItem = new Inventory
                            {
                                ProductId = detail.ProductId,
                                WarehouseId = stockTake.WarehouseId,
                                Quantity = detail.CountedQuantity.Value,
                                LastUpdated = DateTime.UtcNow
                            };
                            _context.Inventories.Add(inventoryItem);
                        }
                        else if (inventoryItem != null)
                        {
                            inventoryItem.Quantity = detail.CountedQuantity.Value; // Cập nhật theo số lượng đếm được
                            inventoryItem.LastUpdated = DateTime.UtcNow;
                            _context.Inventories.Update(inventoryItem);
                        }
                        // Nếu inventoryItem null và CountedQuantity là 0 thì không cần làm gì với inventory.

                        // 2. Tạo InventoryTransaction
                        var transactionQuantity = detail.CountedQuantity.Value - detail.SystemQuantity; // Chênh lệch để ghi vào transaction
                        var inventoryTransaction = new InventoryTransaction
                        {
                            TransactionCode = $"TRN-STK-{stockTake.Code}-{detail.ProductId}",
                            ProductId = detail.ProductId,
                            // Nếu tăng tồn (Counted > System) => DestinationWarehouseId
                            // Nếu giảm tồn (Counted < System) => SourceWarehouseId
                            SourceWarehouseId = transactionQuantity < 0 ? stockTake.WarehouseId : (int?)null,
                            DestinationWarehouseId = transactionQuantity > 0 ? stockTake.WarehouseId : (int?)null,
                            Quantity = Math.Abs(transactionQuantity), // Luôn là số dương
                            TransactionType = "StockTakeAdjustment",
                            ReferenceId = stockTake.Id,
                            TransactionDate = DateTime.UtcNow,
                            Notes = $"Điều chỉnh kiểm kê {stockTake.Code}. SL hệ thống: {detail.SystemQuantity}, SL thực tế: {detail.CountedQuantity.Value}, Chênh lệch: {detail.Variance.Value * -1}. Ghi chú SP: {detail.Notes}",
                            CreatedBy = completedByUserId
                        };
                        _context.InventoryTransactions.Add(inventoryTransaction);
                    }
                    await _stockTakeRepository.UpdateDetailAsync(detail); // Lưu variance
                }

                stockTake.Status = "Completed";
                stockTake.EndDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(completionNotes))
                {
                    stockTake.Notes = string.IsNullOrEmpty(stockTake.Notes) ? completionNotes : $"{stockTake.Notes}\nCompletion Notes: {completionNotes}";
                }
                // Ghi nhận người hoàn thành (nếu cần)
                await _stockTakeRepository.UpdateAsync(stockTake);
                await _context.SaveChangesAsync(); // Lưu tất cả thay đổi

                await transaction.CommitAsync();

                var completedStockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
                return MapToDto(completedStockTake!);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<StockTakeDto> CancelStockTakeAsync(int stockTakeId, int cancelledByUserId)
        {
            var stockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
            if (stockTake == null)
            {
                throw new KeyNotFoundException("Stock take not found.");
            }
            if (stockTake.Status == "Completed")
            {
                throw new InvalidOperationException("Cannot cancel a completed stock take.");
            }

            stockTake.Status = "Cancelled";
            stockTake.EndDate = DateTime.UtcNow; // Ghi nhận ngày hủy
            // Ghi nhận người hủy
            await _stockTakeRepository.UpdateAsync(stockTake);

            var cancelledStockTake = await _stockTakeRepository.GetByIdAsync(stockTakeId);
            return MapToDto(cancelledStockTake!);
        }

        private static StockTakeDto MapToDto(StockTake st)
        {
            return new StockTakeDto
            {
                Id = st.Id,
                Code = st.Code,
                WarehouseId = st.WarehouseId,
                WarehouseName = st.Warehouse?.Name,
                StartDate = st.StartDate,
                EndDate = st.EndDate,
                Status = st.Status,
                Notes = st.Notes,
                CreatedBy = st.CreatedBy,
                CreatedByFullName = st.CreatedByNavigation?.FullName,
                CreatedDate = st.CreatedDate,
                StockTakeDetails = st.StockTakeDetails?.Select(std => new StockTakeDetailDto
                {
                    Id = std.Id,
                    ProductId = std.ProductId,
                    ProductName = std.Product?.Name,
                    ProductCode = std.Product?.Code,
                    ProductUnit = std.Product?.Unit,
                    SystemQuantity = std.SystemQuantity,
                    CountedQuantity = std.CountedQuantity,
                    Variance = std.Variance,
                    Notes = std.Notes
                }).ToList() ?? new List<StockTakeDetailDto>()
            };
        }
    }
}