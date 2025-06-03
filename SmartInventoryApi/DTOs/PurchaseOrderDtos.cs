using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    // DTO cho chi tiết một sản phẩm trong phiếu nhập kho
    public class PurchaseOrderDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int Quantity { get; set; } // Số lượng đặt mua/nhập
        public decimal UnitPrice { get; set; } // Giá nhập đơn vị
        public decimal TotalPrice { get; set; }
        public int ProcessedQuantity { get; set; } // Số lượng đã thực sự xử lý/nhập (có thể dùng sau này)
    }

    // DTO để hiển thị thông tin phiếu nhập kho
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public string Code { get; set; } // Mã phiếu nhập
        public string OrderType { get; set; } // Sẽ là "Purchase"
        public int PartnerId { get; set; } // ID Nhà cung cấp
        public string? PartnerName { get; set; } // Tên Nhà cung cấp
        public int WarehouseId { get; set; } // ID Kho nhập hàng
        public string? WarehouseName { get; set; } // Tên Kho nhập hàng
        public DateTime OrderDate { get; set; } // Ngày đặt hàng/lập phiếu
        public DateTime? DeliveryDate { get; set; } // Ngày dự kiến nhận hàng
        public string Status { get; set; } // Trạng thái: Pending, Approved, Completed, Cancelled
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<PurchaseOrderDetailDto> OrderDetails { get; set; } = new List<PurchaseOrderDetailDto>();
    }

    // DTO cho chi tiết sản phẩm khi tạo phiếu nhập kho
    public class CreatePurchaseOrderDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Unit price must be non-negative.")]
        public decimal UnitPrice { get; set; }
    }

    // DTO để tạo mới phiếu nhập kho
    public class CreatePurchaseOrderDto
    {
        [Required]
        public int PartnerId { get; set; } // ID Nhà cung cấp

        [Required]
        public int WarehouseId { get; set; } // ID Kho nhập hàng

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order detail is required.")]
        public List<CreatePurchaseOrderDetailDto> OrderDetails { get; set; } = new List<CreatePurchaseOrderDetailDto>();
    }

    // DTO để cập nhật phiếu nhập kho (chủ yếu là trước khi phê duyệt)
    public class UpdatePurchaseOrderDto
    {
        [Required]
        public int PartnerId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Có thể cho phép cập nhật chi tiết đơn hàng hoặc không tùy theo quy trình
        // Nếu cho phép, cần logic phức tạp hơn để xử lý thêm/sửa/xóa OrderDetail
        // Ở đây tạm thời không cho cập nhật chi tiết qua DTO này để đơn giản hóa
        // public List<CreatePurchaseOrderDetailDto> OrderDetails { get; set; } = new List<CreatePurchaseOrderDetailDto>();
    }

    // DTO cho các tham số truy vấn phiếu nhập kho
    public class PurchaseOrderQueryParameters
    {
        public int? PartnerId { get; set; }
        public int? WarehouseId { get; set; }
        public string? Status { get; set; } // Pending, Approved, Completed, Cancelled
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; } // Tìm theo mã phiếu

        private const int MaxPageSize = 50;
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
        }

        public string SortBy { get; set; } = "OrderDate";
        public string SortDirection { get; set; } = "desc";
    }
}
