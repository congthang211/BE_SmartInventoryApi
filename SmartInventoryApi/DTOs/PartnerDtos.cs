using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    // DTO để trả về thông tin đối tác
    public class PartnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Supplier" hoặc "Customer"
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // DTO để nhận dữ liệu khi tạo mới đối tác
    public class CreatePartnerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression("^(Supplier|Customer)$", ErrorMessage = "Type must be 'Supplier' or 'Customer'.")]
        public string Type { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; }
    }

    // DTO để nhận dữ liệu khi cập nhật đối tác
    public class UpdatePartnerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression("^(Supplier|Customer)$", ErrorMessage = "Type must be 'Supplier' or 'Customer'.")]
        public string Type { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; }

        public bool IsActive { get; set; }
    }
    public class PartnerOrderHistoryItemDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string OrderType { get; set; } // "Purchase" hoặc "Sale"
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string WarehouseName { get; set; } // Kho liên quan đến đơn hàng
    }

    // DTO cho tham số truy vấn lịch sử đơn hàng của đối tác
    public class PartnerOrderHistoryQueryParameters
    {
        public string? OrderType { get; set; } // "Purchase", "Sale"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }

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