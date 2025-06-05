namespace SmartInventoryApi.DTOs
{
    // DTO cho các chỉ số chính trên Dashboard
    public class DashboardSummaryDto
    {
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; } // Tổng giá trị tồn kho (tính theo giá vốn)
        public int PendingPurchaseOrders { get; set; } // Số phiếu nhập kho đang chờ xử lý
        public int PendingSalesOrders { get; set; }    // Số phiếu xuất kho đang chờ xử lý
        public int LowStockProducts { get; set; }      // Số sản phẩm dưới mức tồn kho tối thiểu
        public int ProductsNearExpiry { get; set; }    // Số sản phẩm sắp hết hạn (nếu có quản lý hạn sử dụng)
        // Thêm các chỉ số khác nếu cần
    }

    // DTO cho một dòng trong báo cáo tồn kho
    public class InventoryReportItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductCategory { get; set; }
        public string Unit { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int CurrentQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalCostValue { get; set; } // CurrentQuantity * CostPrice
        public int MinimumStock { get; set; }
        public bool IsBelowMinimumStock => CurrentQuantity < MinimumStock;
    }

    // DTO cho một dòng trong báo cáo giao dịch kho (nhập/xuất)
    public class InventoryTransactionReportItemDto
    {
        public int TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } // Import, Export, StockTakeAdjustment, etc.
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; } // Số lượng thay đổi (+ cho nhập, - cho xuất)
        public int? SourceWarehouseId { get; set; }
        public string? SourceWarehouseName { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public string? DestinationWarehouseName { get; set; }
        public string? ReferenceOrderCode { get; set; } // Mã phiếu nhập/xuất/kiểm kê liên quan
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string? Notes { get; set; }
    }

    // DTO cho tham số truy vấn báo cáo tồn kho
    public class InventoryReportQueryParameters
    {
        public int? WarehouseId { get; set; }
        public int? CategoryId { get; set; }
        public bool? OnlyLowStock { get; set; } // Chỉ hiển thị sản phẩm dưới tồn kho tối thiểu
        public string? SearchTerm { get; set; } // Tìm theo tên hoặc mã sản phẩm

        private const int MaxPageSize = 100; // Cho phép nhiều hơn cho báo cáo
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
        }
        public string SortBy { get; set; } = "ProductName";
        public string SortDirection { get; set; } = "asc";
    }

    // DTO cho tham số truy vấn báo cáo giao dịch kho
    public class InventoryTransactionReportQueryParameters
    {
        public int? WarehouseId { get; set; }
        public int? ProductId { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }

        private const int MaxPageSize = 100;
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
        }
        public string SortBy { get; set; } = "TransactionDate";
        public string SortDirection { get; set; } = "desc";
    }
}
