using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager được xem báo cáo
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] int? warehouseId)
        {
            var summary = await _reportService.GetDashboardSummaryAsync(warehouseId);
            return Ok(summary);
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport([FromQuery] InventoryReportQueryParameters queryParameters)
        {
            var report = await _reportService.GetInventoryReportAsync(queryParameters);
            return Ok(report);
        }

        [HttpGet("inventory-transactions")]
        public async Task<IActionResult> GetInventoryTransactionReport([FromQuery] InventoryTransactionReportQueryParameters queryParameters)
        {
            var report = await _reportService.GetInventoryTransactionReportAsync(queryParameters);
            return Ok(report);
        }

        // Thêm các endpoint cho các báo cáo khác nếu cần
        // Ví dụ: /api/reports/product-performance, /api/reports/order-summary
    }
}
