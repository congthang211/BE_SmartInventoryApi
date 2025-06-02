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
}