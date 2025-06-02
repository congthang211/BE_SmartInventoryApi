using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền xem log
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivityLogs([FromQuery] ActivityLogQueryParameters queryParameters)
        {
            var paginatedLogs = await _activityLogService.GetActivityLogsAsync(queryParameters);
            return Ok(paginatedLogs);
        }
    }
}