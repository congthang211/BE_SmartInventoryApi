using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
        // Đảm bảo tham số là CreateWarehouseDto
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto warehouseDto);
        // Đảm bảo tham số là UpdateWarehouseDto
        Task UpdateWarehouseAsync(int id, UpdateWarehouseDto warehouseDto);
        Task DeleteWarehouseAsync(int id);
    }
}