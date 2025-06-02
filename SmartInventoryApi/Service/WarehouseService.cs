using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto warehouseDto)
        {
            if (await _warehouseRepository.NameExistsAsync(warehouseDto.Name))
            {
                throw new InvalidOperationException("Warehouse name already exists.");
            }

            var warehouse = new Warehouse
            {
                Name = warehouseDto.Name,
                Location = warehouseDto.Location,
                Description = warehouseDto.Description,
                IsActive = true
            };

            var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);
            return MapToDto(createdWarehouse);
        }

        public async Task DeleteWarehouseAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }
            await _warehouseRepository.DeleteAsync(warehouse);
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return warehouses.Select(MapToDto);
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            return warehouse == null ? null : MapToDto(warehouse);
        }

        public async Task UpdateWarehouseAsync(int id, UpdateWarehouseDto warehouseDto)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException("Warehouse not found.");
            }

            if (await _warehouseRepository.NameExistsAsync(warehouseDto.Name, id))
            {
                throw new InvalidOperationException("Warehouse name already exists.");
            }

            warehouse.Name = warehouseDto.Name;
            warehouse.Location = warehouseDto.Location;
            warehouse.Description = warehouseDto.Description;
            warehouse.IsActive = warehouseDto.IsActive;

            await _warehouseRepository.UpdateAsync(warehouse);
        }

        private static WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                Description = warehouse.Description,
                IsActive = warehouse.IsActive
            };
        }
    }
}