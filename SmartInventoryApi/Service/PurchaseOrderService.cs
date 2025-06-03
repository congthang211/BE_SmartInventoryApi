using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly InventoryManagementDbContext _context; // Dùng cho transaction

        public PurchaseOrderService(
            IPurchaseOrderRepository purchaseOrderRepository,
            IProductRepository productRepository,
            IPartnerRepository partnerRepository,
            IWarehouseRepository warehouseRepository,
            InventoryManagementDbContext context)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _productRepository = productRepository;
            _partnerRepository = partnerRepository;
            _warehouseRepository = warehouseRepository;
            _context = context;
        }

        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, int createdByUserId)
        {
            var supplier = await _partnerRepository.GetByIdAsync(dto.PartnerId);
            if (supplier == null || supplier.Type != "Supplier")
            {
                throw new KeyNotFoundException("Supplier not found or partner is not a supplier.");
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }

            var order = new Order
            {
                Code = await _purchaseOrderRepository.GetNextPurchaseOrderCodeAsync(),
                OrderType = "Purchase",
                PartnerId = dto.PartnerId,
                WarehouseId = dto.WarehouseId,
                OrderDate = dto.OrderDate,
                DeliveryDate = dto.DeliveryDate,
                Status = "Pending", // Trạng thái ban đầu
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;
            foreach (var detailDto in dto.OrderDetails)
            {
                var product = await _productRepository.GetByIdAsync(detailDto.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {detailDto.ProductId} not found.");
                }
                var orderDetail = new OrderDetail
                {
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice, // Giá nhập
                    TotalPrice = detailDto.Quantity * detailDto.UnitPrice,
                    ProcessedQuantity = 0 // Ban đầu chưa xử lý
                };
                order.OrderDetails.Add(orderDetail);
                totalAmount += orderDetail.TotalPrice;
            }
            order.TotalAmount = totalAmount;

            var createdOrder = await _purchaseOrderRepository.CreateAsync(order);

            // Tải lại để có đủ thông tin cho DTO
            var fullOrder = await _purchaseOrderRepository.GetByIdAsync(createdOrder.Id);
            return MapToDto(fullOrder!);
        }

        public async Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(int id, UpdatePurchaseOrderDto dto, int updatedByUserId)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Purchase order not found.");
            }
            if (order.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending purchase orders can be updated.");
            }

            var supplier = await _partnerRepository.GetByIdAsync(dto.PartnerId);
            if (supplier == null || supplier.Type != "Supplier")
            {
                throw new KeyNotFoundException("Supplier not found or partner is not a supplier.");
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }

            order.PartnerId = dto.PartnerId;
            order.WarehouseId = dto.WarehouseId;
            order.OrderDate = dto.OrderDate;
            order.DeliveryDate = dto.DeliveryDate;
            order.Notes = dto.Notes;
            // Ghi nhận người cập nhật (nếu cần)
            // order.LastModifiedBy = updatedByUserId;
            // order.LastModifiedDate = DateTime.UtcNow;

            // Việc cập nhật OrderDetails phức tạp hơn, có thể cần xóa cũ thêm mới
            // hoặc so sánh từng dòng. Tạm thời ở đây chỉ cập nhật header.
            // Nếu cho phép cập nhật chi tiết, cần tính lại TotalAmount.

            await _purchaseOrderRepository.UpdateAsync(order);

            var updatedFullOrder = await _purchaseOrderRepository.GetByIdAsync(id);
            return MapToDto(updatedFullOrder!);
        }

        public async Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(int id, int approvedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _purchaseOrderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException("Purchase order not found.");
                }
                if (order.Status != "Pending")
                {
                    throw new InvalidOperationException("Only pending purchase orders can be approved.");
                }

                order.Status = "Approved"; // Hoặc "Completed" nếu quy trình đơn giản
                // Ghi nhận người duyệt (nếu cần)
                // order.ApprovedBy = approvedByUserId;
                // order.ApprovedDate = DateTime.UtcNow;

                foreach (var detail in order.OrderDetails)
                {
                    // 1. Cập nhật Inventory
                    var inventoryItem = await _context.Inventories
                                                    .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId && i.WarehouseId == order.WarehouseId);
                    if (inventoryItem == null)
                    {
                        inventoryItem = new Inventory
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = order.WarehouseId,
                            Quantity = detail.Quantity,
                            LastUpdated = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventoryItem);
                    }
                    else
                    {
                        inventoryItem.Quantity += detail.Quantity;
                        inventoryItem.LastUpdated = DateTime.UtcNow;
                        _context.Inventories.Update(inventoryItem);
                    }

                    // 2. Tạo InventoryTransaction
                    var inventoryTransaction = new InventoryTransaction
                    {
                        TransactionCode = $"TRN-PNK-{order.Code}-{detail.ProductId}", // Mã giao dịch
                        ProductId = detail.ProductId,
                        DestinationWarehouseId = order.WarehouseId, // Nhập vào kho này
                        Quantity = detail.Quantity,
                        TransactionType = "Import", // Loại giao dịch
                        ReferenceId = order.Id, // Tham chiếu đến Order ID
                        TransactionDate = DateTime.UtcNow,
                        Notes = $"Nhập hàng theo phiếu {order.Code}",
                        CreatedBy = approvedByUserId
                    };
                    _context.InventoryTransactions.Add(inventoryTransaction);

                    // 3. Cập nhật ProcessedQuantity trong OrderDetail
                    detail.ProcessedQuantity = detail.Quantity; // Giả sử xử lý hết
                    _context.OrderDetails.Update(detail);
                }

                // Nếu sau khi duyệt, đơn hàng coi như hoàn thành
                if (order.Status == "Approved") // Điều kiện này có thể thay đổi tùy quy trình
                {
                    order.Status = "Completed";
                }

                await _purchaseOrderRepository.UpdateAsync(order); // Lưu thay đổi trạng thái Order
                await _context.SaveChangesAsync(); // Lưu thay đổi Inventory, InventoryTransaction, OrderDetail

                await transaction.CommitAsync();

                var approvedFullOrder = await _purchaseOrderRepository.GetByIdAsync(id);
                return MapToDto(approvedFullOrder!);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PurchaseOrderDto> CancelPurchaseOrderAsync(int id, int cancelledByUserId)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Purchase order not found.");
            }
            if (order.Status != "Pending" && order.Status != "Approved") // Chỉ hủy khi đang chờ hoặc đã duyệt nhưng chưa hoàn tất sâu
            {
                throw new InvalidOperationException($"Cannot cancel purchase order with status '{order.Status}'.");
            }

            order.Status = "Cancelled";
            // Ghi nhận người hủy (nếu cần)
            // order.CancelledBy = cancelledByUserId;
            // order.CancelledDate = DateTime.UtcNow;
            await _purchaseOrderRepository.UpdateAsync(order);

            var cancelledFullOrder = await _purchaseOrderRepository.GetByIdAsync(id);
            return MapToDto(cancelledFullOrder!);
        }


        public async Task<PaginatedResponseDto<PurchaseOrderDto>> GetAllPurchaseOrdersAsync(PurchaseOrderQueryParameters queryParameters)
        {
            var orders = await _purchaseOrderRepository.GetAllAsync(queryParameters);
            var totalCount = await _purchaseOrderRepository.GetTotalCountAsync(queryParameters);
            var orderDtos = orders.Select(MapToDto);
            return new PaginatedResponseDto<PurchaseOrderDto>(orderDtos, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }

        public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(int id)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        private static PurchaseOrderDto MapToDto(Order order)
        {
            return new PurchaseOrderDto
            {
                Id = order.Id,
                Code = order.Code,
                OrderType = order.OrderType,
                PartnerId = order.PartnerId,
                PartnerName = order.Partner?.Name,
                WarehouseId = order.WarehouseId,
                WarehouseName = order.Warehouse?.Name,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                CreatedBy = order.CreatedBy,
                CreatedByFullName = order.CreatedByNavigation?.FullName,
                CreatedDate = order.CreatedDate,
                OrderDetails = order.OrderDetails.Select(od => new PurchaseOrderDetailDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name,
                    ProductCode = od.Product?.Code,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.TotalPrice,
                    ProcessedQuantity = od.ProcessedQuantity
                }).ToList()
            };
        }
    }
}
