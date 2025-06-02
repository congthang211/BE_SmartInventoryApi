using System.ComponentModel.DataAnnotations;

namespace SmartInventoryApi.DTOs
{
    // DTO để trả về thông tin kho
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO để nhận dữ liệu khi tạo mới kho
    public class CreateWarehouseDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }
    }

    // DTO để nhận dữ liệu khi cập nhật kho
    public class UpdateWarehouseDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}