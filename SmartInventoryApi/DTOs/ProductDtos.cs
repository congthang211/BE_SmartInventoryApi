using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string? Barcode { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // Tên nhóm hàng
        public string Unit { get; set; } // Đơn vị tính
        public int? DefaultSupplierId { get; set; }
        public string? DefaultSupplierName { get; set; } // Tên nhà cung cấp mặc định
        public decimal CostPrice { get; set; } // Giá vốn
        public decimal SellingPrice { get; set; } // Giá bán
        public int MinimumStock { get; set; }
        public int? MaximumStock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; } // UserId của người tạo
        public string? CreatedByFullName { get; set; } // Tên người tạo
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; }

        public int? DefaultSupplierId { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal SellingPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaximumStock { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductDto
    {
        // Code thường không cho phép cập nhật hoặc cần xử lý đặc biệt. Ở đây ta không cho cập nhật Code.
        // Nếu muốn cho cập nhật Barcode, Name, cần đảm bảo tính duy nhất nếu có yêu cầu.

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; }

        public int? DefaultSupplierId { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal SellingPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaximumStock { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }

    // DTO cho các tham số truy vấn sản phẩm
    public class ProductQueryParameters
    {
        public string? SearchTerm { get; set; } // Tìm theo Name hoặc Code
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; } = true; // Mặc định chỉ lấy các sản phẩm active

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

        public string SortBy { get; set; } = "Name"; // Mặc định sắp xếp theo Name
        public string SortDirection { get; set; } = "asc"; // Mặc định tăng dần
    }
}