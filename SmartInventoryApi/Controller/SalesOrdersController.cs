using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;
using System.Security.Claims;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Manager và Admin có quyền quản lý phiếu xuất
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;

        public SalesOrdersController(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesOrders([FromQuery] SalesOrderQueryParameters queryParameters)
        {
            var orders = await _salesOrderService.GetAllSalesOrdersAsync(queryParameters);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalesOrderById(int id)
        {
            var order = await _salesOrderService.GetSalesOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound("Sales order not found.");
            }
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesOrder([FromBody] CreateSalesOrderDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var newOrder = await _salesOrderService.CreateSalesOrderAsync(createDto, userId);
                return CreatedAtAction(nameof(GetSalesOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // Ví dụ: không đủ tồn kho
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSalesOrder(int id, [FromBody] UpdateSalesOrderDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var updatedOrder = await _salesOrderService.UpdateSalesOrderAsync(id, updateDto, userId);
                return Ok(updatedOrder);
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

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveSalesOrder(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var approvedOrder = await _salesOrderService.ApproveSalesOrderAsync(id, userId);
                return Ok(approvedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex) // Ví dụ: không đủ tồn kho, sai trạng thái
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSalesOrder(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var cancelledOrder = await _salesOrderService.CancelSalesOrderAsync(id, userId);
                return Ok(cancelledOrder);
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
    }
}
