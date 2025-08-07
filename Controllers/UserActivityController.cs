using Microsoft.AspNetCore.Mvc;
using PersonaXFleet.Services;

namespace PersonaXFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _activityService;

        public UserActivityController(IUserActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 50)
        {
            var activities = await _activityService.GetRecentActivitiesAsync(limit);
            return Ok(activities);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserActivities(
            string userId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null, 
            [FromQuery] int limit = 100)
        {
            var activities = await _activityService.GetUserActivitiesAsync(userId, fromDate, toDate, limit);
            return Ok(activities);
        }

        [HttpGet("module/{module}")]
        public async Task<IActionResult> GetModuleActivities(
            string module, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var activities = await _activityService.GetModuleActivitiesAsync(module, fromDate, toDate);
            return Ok(activities);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetActivityStats(
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var stats = await _activityService.GetActivityStatsAsync(fromDate, toDate);
            return Ok(stats);
        }

        [HttpGet("user-summary/{userId}")]
        public async Task<IActionResult> GetUserActivitySummary(string userId)
        {
            var summary = await _activityService.GetUserActivitySummaryAsync(userId);
            return Ok(summary);
        }

        [HttpGet("most-active-users")]
        public async Task<IActionResult> GetMostActiveUsers([FromQuery] int limit = 10)
        {
            var users = await _activityService.GetMostActiveUsersAsync(limit);
            return Ok(users);
        }

        [HttpGet("popular-pages")]
        public async Task<IActionResult> GetPopularPages([FromQuery] int limit = 10)
        {
            var pages = await _activityService.GetPopularPagesAsync(limit);
            return Ok(pages);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetActivityTrends([FromQuery] int days = 30)
        {
            var trends = await _activityService.GetActivityTrendsAsync(days);
            return Ok(trends);
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogActivity([FromBody] LogActivityRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("UserId is required");

            await _activityService.LogActivityAsync(
                request.UserId,
                request.ActivityType,
                request.Module,
                request.Description,
                request.EntityType,
                request.EntityId,
                request.Details
            );

            return Ok(new { message = "Activity logged successfully" });
        }

        [HttpGet("my-activities")]
        public async Task<IActionResult> GetMyActivities(
            [FromQuery] string userId,
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null, 
            [FromQuery] int limit = 100)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required");

            var activities = await _activityService.GetUserActivitiesAsync(userId, fromDate, toDate, limit);
            return Ok(activities);
        }

        [HttpGet("my-summary")]
        public async Task<IActionResult> GetMyActivitySummary([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required");

            var summary = await _activityService.GetUserActivitySummaryAsync(userId);
            return Ok(summary);
        }
    }

    public class LogActivityRequest
    {
        public string UserId { get; set; }
        public string ActivityType { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public object Details { get; set; }
    }
} 