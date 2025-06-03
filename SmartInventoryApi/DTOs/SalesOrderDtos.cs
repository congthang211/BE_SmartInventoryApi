using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    // DTO cho chi tiết một sản phẩm trong phiếu xuất kho
    public class SalesOrderDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int Quantity { get; set; } // Số lượng đặt mua/xuất
        public decimal UnitPrice { get; set; } // Giá bán đơn vị
        public decimal TotalPrice { get; set; }
        public int ProcessedQuantity { get; set; } // Số lượng đã thực sự xử lý/xuất
    }

    // DTO để hiển thị thông tin phiếu xuất kho
    public class SalesOrderDto
    {
        public int Id { get; set; }
        public string Code { get; set; } // Mã phiếu xuất
        public string OrderType { get; set; } // Sẽ là "Sale"
        public int PartnerId { get; set; } // ID Khách hàng
        public string? PartnerName { get; set; } // Tên Khách hàng
        public int WarehouseId { get; set; } // ID Kho xuất hàng
        public string? WarehouseName { get; set; } // Tên Kho xuất hàng
        public DateTime OrderDate { get; set; } // Ngày đặt hàng/lập phiếu
        public DateTime? DeliveryDate { get; set; } // Ngày dự kiến giao hàng
        public string Status { get; set; } // Trạng thái: Pending, Approved, Shipped/Completed, Cancelled
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<SalesOrderDetailDto> OrderDetails { get; set; } = new List<SalesOrderDetailDto>();
    }

    // DTO cho chi tiết sản phẩm khi tạo phiếu xuất kho
    public class CreateSalesOrderDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        // Giá bán có thể lấy từ sản phẩm hoặc nhập thủ công tùy quy trình
        // Nếu lấy từ sản phẩm, không cần trường này ở đây.
        // Nếu cho phép ghi đè giá, thì cần thêm.
        // Tạm thời giả định giá bán lấy từ thông tin sản phẩm.
        // public decimal UnitPrice { get; set; } 
    }

    // DTO để tạo mới phiếu xuất kho
    public class CreateSalesOrderDto
    {
        [Required]
        public int PartnerId { get; set; } // ID Khách hàng

        [Required]
        public int WarehouseId { get; set; } // ID Kho xuất hàng

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order detail is required.")]
        public List<CreateSalesOrderDetailDto> OrderDetails { get; set; } = new List<CreateSalesOrderDetailDto>();
    }

    // DTO để cập nhật phiếu xuất kho
    public class UpdateSalesOrderDto
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
        // Tương tự phiếu nhập, việc cập nhật chi tiết sẽ phức tạp, tạm thời bỏ qua.
    }

    // DTO cho các tham số truy vấn phiếu xuất kho
    public class SalesOrderQueryParameters
    {
        public int? PartnerId { get; set; } // CustomerId
        public int? WarehouseId { get; set; }
        public string? Status { get; set; } // Pending, Approved, Shipped, Cancelled
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
