using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;
using System.Security.Claims;

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdString, out var requestingUserId) || string.IsNullOrEmpty(userRole))
            {
                return Unauthorized("User claims are missing, invalid, or user role is not defined in token.");
            }

            var paginatedLogs = await _activityLogService.GetActivityLogsAsync(queryParameters, requestingUserId, userRole);
            return Ok(paginatedLogs);
        }
    }
}
