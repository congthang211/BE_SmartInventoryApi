using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;
using System.Security.Claims;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Manager và Admin có quyền quản lý kiểm kê
    public class StockTakesController : ControllerBase
    {
        private readonly IStockTakeService _stockTakeService;

        public StockTakesController(IStockTakeService stockTakeService)
        {
            _stockTakeService = stockTakeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStockTakes([FromQuery] StockTakeQueryParameters queryParameters)
        {
            var stockTakes = await _stockTakeService.GetAllStockTakesAsync(queryParameters);
            return Ok(stockTakes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockTakeById(int id)
        {
            var stockTake = await _stockTakeService.GetStockTakeByIdAsync(id);
            if (stockTake == null)
            {
                return NotFound("Stock take not found.");
            }
            return Ok(stockTake);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockTake([FromBody] CreateStockTakeDto createDto)
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
                var newStockTake = await _stockTakeService.CreateStockTakeAsync(createDto, userId);
                return CreatedAtAction(nameof(GetStockTakeById), new { id = newStockTake.Id }, newStockTake);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/record-counts")] // Staff hoặc Manager có thể ghi nhận số đếm
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> RecordStockTakeCounts(int id, [FromBody] RecordStockTakeCountsDto countsDto)
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
                var updatedStockTake = await _stockTakeService.RecordStockTakeCountsAsync(id, countsDto, userId);
                return Ok(updatedStockTake);
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

        [HttpPost("{id}/complete")] // Chỉ Manager/Admin được hoàn thành và điều chỉnh kho
        public async Task<IActionResult> CompleteStockTake(int id, [FromBody] string? completionNotes)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var completedStockTake = await _stockTakeService.CompleteStockTakeAsync(id, userId, completionNotes);
                return Ok(completedStockTake);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex) // Ví dụ: chưa đếm hết, sai trạng thái
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelStockTake(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var cancelledStockTake = await _stockTakeService.CancelStockTakeAsync(id, userId);
                return Ok(cancelledStockTake);
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
