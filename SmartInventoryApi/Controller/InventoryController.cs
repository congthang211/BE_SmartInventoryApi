using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.Services;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Staff")] // Tất cả các vai trò này đều có thể cần tra cứu tồn kho
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("product/{productId}/warehouse/{warehouseId}")]
        public async Task<IActionResult> GetProductStockInWarehouse(int productId, int warehouseId)
        {
            var stockInfo = await _inventoryService.GetProductStockAsync(productId, warehouseId);
            if (stockInfo == null)
            {
                // Có thể trả về NotFound nếu không có bản ghi Inventory, 
                // hoặc trả về OK với Quantity = 0 nếu muốn ngầm định là sản phẩm có thể có nhưng chưa nhập.
                // Hiện tại service trả về null nếu không có Inventory, nên NotFound là phù hợp.
                return NotFound($"Stock information not found for Product ID {productId} in Warehouse ID {warehouseId}.");
            }
            return Ok(stockInfo);
        }
    }
}
