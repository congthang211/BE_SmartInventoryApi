namespace SmartInventoryApi.DTOs
{
    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int CurrentQuantity { get; set; }
        public string Unit { get; set; }
        public int MinimumStock { get; set; }
        public bool IsBelowMinimumStock => CurrentQuantity < MinimumStock;
    }
}
