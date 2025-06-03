using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    // DTO cho chi tiết một sản phẩm trong phiếu kiểm kê
    public class StockTakeDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductUnit { get; set; }
        public int SystemQuantity { get; set; } // Số lượng hệ thống ghi nhận tại thời điểm tạo phiếu
        public int? CountedQuantity { get; set; } // Số lượng thực tế kiểm đếm được
        public int? Variance { get; set; } // Chênh lệch: SystemQuantity - CountedQuantity
        public string? Notes { get; set; } // Ghi chú cho từng dòng sản phẩm
    }

    // DTO để hiển thị thông tin phiếu kiểm kê
    public class StockTakeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } // Mã phiếu kiểm kê
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime StartDate { get; set; } // Ngày bắt đầu kiểm kê
        public DateTime? EndDate { get; set; } // Ngày kết thúc/hoàn thành kiểm kê
        public string Status { get; set; } // Trạng thái: Draft, InProgress, PendingApproval, Completed, Cancelled
        public string? Notes { get; set; } // Ghi chú chung cho phiếu
        public int CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<StockTakeDetailDto> StockTakeDetails { get; set; } = new List<StockTakeDetailDto>();
    }

    // DTO để tạo mới phiếu kiểm kê
    public class CreateStockTakeDto
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Danh sách ProductId để kiểm kê (tùy chọn). 
        // Nếu null hoặc rỗng, sẽ kiểm kê toàn bộ sản phẩm có tồn trong kho đó.
        public List<int>? ProductIdsToTake { get; set; }
    }

    // DTO để cập nhật số lượng đếm được cho một sản phẩm trong phiếu kiểm kê
    public class UpdateStockTakeDetailItemDto
    {
        [Required]
        public int ProductId { get; set; } // Dùng ProductId để xác định dòng cần cập nhật

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Counted quantity must be non-negative.")]
        public int CountedQuantity { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }
    }

    // DTO để cập nhật (ghi nhận) kết quả kiểm đếm cho phiếu kiểm kê
    public class RecordStockTakeCountsDto
    {
        [Required]
        [MinLength(1)]
        public List<UpdateStockTakeDetailItemDto> Counts { get; set; } = new List<UpdateStockTakeDetailItemDto>();

        [StringLength(500)]
        public string? OverallNotes { get; set; } // Ghi chú chung khi ghi nhận
    }

    // DTO cho các tham số truy vấn phiếu kiểm kê
    public class StockTakeQueryParameters
    {
        public int? WarehouseId { get; set; }
        public string? Status { get; set; } // Draft, InProgress, PendingApproval, Completed, Cancelled
        public DateTime? StartDatePeriod { get; set; } // Lọc theo khoảng ngày bắt đầu
        public DateTime? EndDatePeriod { get; set; }   // Lọc theo khoảng ngày bắt đầu
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

        public string SortBy { get; set; } = "StartDate";
        public string SortDirection { get; set; } = "desc";
    }
}
