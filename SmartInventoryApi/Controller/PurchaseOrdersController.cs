﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;
using System.Security.Claims;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực cho tất cả, quyền chi tiết sẽ đặt ở từng phương thức
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff")] // Mở quyền cho Staff
        public async Task<IActionResult> GetAllPurchaseOrders([FromQuery] PurchaseOrderQueryParameters queryParameters)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var orders = await _purchaseOrderService.GetAllPurchaseOrdersAsync(queryParameters, userId, userRole);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")] // Mở quyền cho Staff
        public async Task<IActionResult> GetPurchaseOrderById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id, userId, userRole);
            if (order == null)
            {
                return NotFound("Purchase order not found or you do not have permission to view it.");
            }
            return Ok(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")] // Mở quyền cho Staff
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto createDto)
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
                var newOrder = await _purchaseOrderService.CreatePurchaseOrderAsync(createDto, userId);
                return CreatedAtAction(nameof(GetPurchaseOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message); // Ví dụ: Product, Partner, Warehouse không tồn tại
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Ví dụ: Lỗi logic nghiệp vụ
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Staff không có quyền này
        public async Task<IActionResult> UpdatePurchaseOrder(int id, [FromBody] UpdatePurchaseOrderDto updateDto)
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
                var updatedOrder = await _purchaseOrderService.UpdatePurchaseOrderAsync(id, updateDto, userId);
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
        [Authorize(Roles = "Admin,Manager")] // Staff không có quyền này
        public async Task<IActionResult> ApprovePurchaseOrder(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var approvedOrder = await _purchaseOrderService.ApprovePurchaseOrderAsync(id, userId);
                return Ok(approvedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Ví dụ: đơn hàng không ở trạng thái "Pending"
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Manager")] // Staff không có quyền này
        public async Task<IActionResult> CancelPurchaseOrder(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var cancelledOrder = await _purchaseOrderService.CancelPurchaseOrderAsync(id, userId);
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