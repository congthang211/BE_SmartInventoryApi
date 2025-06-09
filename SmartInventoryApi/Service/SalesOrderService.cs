using Microsoft.EntityFrameworkCore;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly InventoryManagementDbContext _context; // For transaction

        public SalesOrderService(
            ISalesOrderRepository salesOrderRepository,
            IProductRepository productRepository,
            IPartnerRepository partnerRepository,
            IWarehouseRepository warehouseRepository,
            InventoryManagementDbContext context)
        {
            _salesOrderRepository = salesOrderRepository;
            _productRepository = productRepository;
            _partnerRepository = partnerRepository;
            _warehouseRepository = warehouseRepository;
            _context = context;
        }

        public async Task<PaginatedResponseDto<SalesOrderDto>> GetAllSalesOrdersAsync(
            SalesOrderQueryParameters queryParameters,
            int requestingUserId,
            string requestingUserRole)
        {
            // Logic bảo mật: Nếu người dùng là Staff, bắt buộc lọc theo ID của họ
            if (requestingUserRole == "Staff")
            {
                queryParameters.CreatedByUserId = requestingUserId;
            }

            var orders = await _salesOrderRepository.GetAllAsync(queryParameters);
            var totalCount = await _salesOrderRepository.GetTotalCountAsync(queryParameters);
            var orderDtos = orders.Select(MapToDto);
            return new PaginatedResponseDto<SalesOrderDto>(orderDtos, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }

        public async Task<SalesOrderDto?> GetSalesOrderByIdAsync(
            int id,
            int requestingUserId,
            string requestingUserRole)
        {
            var order = await _salesOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }

            // Logic bảo mật: Nếu người dùng là Staff, kiểm tra xem họ có phải là người tạo phiếu này không
            if (requestingUserRole == "Staff" && order.CreatedBy != requestingUserId)
            {
                return null;
            }

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> CreateSalesOrderAsync(CreateSalesOrderDto dto, int createdByUserId)
        {
            var customer = await _partnerRepository.GetByIdAsync(dto.PartnerId);
            if (customer == null || customer.Type != "Customer")
            {
                throw new KeyNotFoundException("Customer not found or partner is not a customer.");
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }

            var order = new Order
            {
                Code = await _salesOrderRepository.GetNextSalesOrderCodeAsync(),
                OrderType = "Sale",
                PartnerId = dto.PartnerId,
                WarehouseId = dto.WarehouseId,
                OrderDate = dto.OrderDate,
                DeliveryDate = dto.DeliveryDate,
                Status = "Pending", // Initial status
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
                if (!product.IsActive)
                {
                    throw new InvalidOperationException($"Product '{product.Name}' is not active.");
                }

                // Kiểm tra tồn kho (đơn giản, có thể cần logic phức tạp hơn cho đặt trước, v.v.)
                var inventoryItem = await _context.Inventories
                                                .FirstOrDefaultAsync(i => i.ProductId == detailDto.ProductId && i.WarehouseId == dto.WarehouseId);
                if (inventoryItem == null || inventoryItem.Quantity < detailDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product '{product.Name}' in warehouse '{warehouse.Name}'. Available: {inventoryItem?.Quantity ?? 0}, Requested: {detailDto.Quantity}");
                }


                var orderDetail = new OrderDetail
                {
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = product.SellingPrice, // Lấy giá bán từ sản phẩm
                    TotalPrice = detailDto.Quantity * product.SellingPrice,
                    ProcessedQuantity = 0
                };
                order.OrderDetails.Add(orderDetail);
                totalAmount += orderDetail.TotalPrice;
            }
            order.TotalAmount = totalAmount;

            var createdOrder = await _salesOrderRepository.CreateAsync(order);
            var fullOrder = await _salesOrderRepository.GetByIdAsync(createdOrder.Id);
            return MapToDto(fullOrder!);
        }

        public async Task<SalesOrderDto> UpdateSalesOrderAsync(int id, UpdateSalesOrderDto dto, int updatedByUserId)
        {
            var order = await _salesOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Sales order not found.");
            }
            if (order.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending sales orders can be updated.");
            }

            var customer = await _partnerRepository.GetByIdAsync(dto.PartnerId);
            if (customer == null || customer.Type != "Customer")
            {
                throw new KeyNotFoundException("Customer not found or partner is not a customer.");
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
            // Ghi nhận người cập nhật...

            // Lưu ý: Nếu cho phép cập nhật OrderDetails, logic ở đây sẽ phức tạp hơn nhiều.
            // Cần kiểm tra lại tồn kho cho từng sản phẩm nếu số lượng thay đổi.
            // Tính lại TotalAmount.

            await _salesOrderRepository.UpdateAsync(order);
            var updatedFullOrder = await _salesOrderRepository.GetByIdAsync(id);
            return MapToDto(updatedFullOrder!);
        }

        public async Task<SalesOrderDto> ApproveSalesOrderAsync(int id, int approvedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _salesOrderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException("Sales order not found.");
                }
                if (order.Status != "Pending")
                {
                    throw new InvalidOperationException("Only pending sales orders can be approved.");
                }

                order.Status = "Approved"; // Hoặc "Processing"
                // Ghi nhận người duyệt...

                foreach (var detail in order.OrderDetails)
                {
                    // 1. Kiểm tra lại tồn kho trước khi trừ
                    var inventoryItem = await _context.Inventories
                                                    .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId && i.WarehouseId == order.WarehouseId);
                    if (inventoryItem == null || inventoryItem.Quantity < detail.Quantity)
                    {
                        // Lỗi này không nên xảy ra nếu đã kiểm tra khi tạo, nhưng vẫn cần phòng ngừa
                        throw new InvalidOperationException($"Insufficient stock for product ID {detail.ProductId} at approval time. Available: {inventoryItem?.Quantity ?? 0}, Requested: {detail.Quantity}");
                    }

                    // 2. Cập nhật Inventory (trừ tồn kho)
                    inventoryItem.Quantity -= detail.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;
                    _context.Inventories.Update(inventoryItem);

                    // 3. Tạo InventoryTransaction
                    var inventoryTransaction = new InventoryTransaction
                    {
                        TransactionCode = $"TRN-PXK-{order.Code}-{detail.ProductId}",
                        ProductId = detail.ProductId,
                        SourceWarehouseId = order.WarehouseId, // Xuất từ kho này
                        Quantity = detail.Quantity,
                        TransactionType = "Export",
                        ReferenceId = order.Id,
                        TransactionDate = DateTime.UtcNow,
                        Notes = $"Xuất hàng theo phiếu {order.Code}",
                        CreatedBy = approvedByUserId
                    };
                    _context.InventoryTransactions.Add(inventoryTransaction);

                    // 4. Cập nhật ProcessedQuantity trong OrderDetail
                    detail.ProcessedQuantity = detail.Quantity;
                    _context.OrderDetails.Update(detail);
                }

                // Cập nhật trạng thái cuối cùng của đơn hàng, ví dụ: "Shipped" hoặc "Completed"
                // Tùy thuộc vào quy trình có bước giao hàng riêng hay không.
                // Tạm thời đặt là "Completed" sau khi duyệt và xử lý kho.
                order.Status = "Completed";

                await _salesOrderRepository.UpdateAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var approvedFullOrder = await _salesOrderRepository.GetByIdAsync(id);
                return MapToDto(approvedFullOrder!);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SalesOrderDto> CancelSalesOrderAsync(int id, int cancelledByUserId)
        {
            var order = await _salesOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Sales order not found.");
            }
            // Chỉ cho phép hủy khi đang Pending hoặc Approved (nếu chưa thực sự xuất kho)
            if (order.Status != "Pending" && order.Status != "Approved")
            {
                throw new InvalidOperationException($"Cannot cancel sales order with status '{order.Status}'.");
            }

            // Nếu đơn hàng đã "Approved" và đã trừ kho, việc hủy sẽ phức tạp hơn (cần hoàn lại tồn kho).
            // Tạm thời, giả sử nếu "Approved" mà hủy thì chỉ đổi status, không hoàn kho tự động ở đây.
            // Logic hoàn kho nếu hủy đơn đã duyệt nên là một quy trình riêng hoặc nghiệp vụ phức tạp hơn.

            order.Status = "Cancelled";
            // Ghi nhận người hủy...
            await _salesOrderRepository.UpdateAsync(order);

            var cancelledFullOrder = await _salesOrderRepository.GetByIdAsync(id);
            return MapToDto(cancelledFullOrder!);
        }

        private static SalesOrderDto MapToDto(Order order)
        {
            return new SalesOrderDto
            {
                Id = order.Id,
                Code = order.Code,
                OrderType = order.OrderType,
                PartnerId = order.PartnerId,
                PartnerName = order.Partner?.Name, // Customer Name
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
                OrderDetails = order.OrderDetails.Select(od => new SalesOrderDetailDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name,
                    ProductCode = od.Product?.Code,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice, // Giá bán
                    TotalPrice = od.TotalPrice,
                    ProcessedQuantity = od.ProcessedQuantity
                }).ToList()
            };
        }
    }
}