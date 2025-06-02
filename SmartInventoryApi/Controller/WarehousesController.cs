using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs; // Đảm bảo using này đã có
using SmartInventoryApi.Services;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouseById(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            if (warehouse == null)
            {
                return NotFound("Warehouse not found.");
            }
            return Ok(warehouse);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto createWarehouseDto)
        {
            try
            {
                var newWarehouse = await _warehouseService.CreateWarehouseAsync(createWarehouseDto);
                return CreatedAtAction(nameof(GetWarehouseById), new { id = newWarehouse.Id }, newWarehouse);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseDto updateWarehouseDto)
        {
            try
            {
                await _warehouseService.UpdateWarehouseAsync(id, updateWarehouseDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                await _warehouseService.DeleteWarehouseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
