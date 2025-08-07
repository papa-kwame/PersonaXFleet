using PersonaXFleet.Models;

namespace PersonaXFleet.Services
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(UserActivity activity);
        Task LogActivityAsync(string userId, string activityType, string module, string description = null, string entityType = null, string entityId = null, object details = null);
        Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100);
        Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 50);
        Task<IEnumerable<UserActivity>> GetModuleActivitiesAsync(string module, DateTime? fromDate = null, DateTime? toDate = null);
        Task<object> GetActivityStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<object>> GetUserActivitySummaryAsync(string userId);
        Task<IEnumerable<object>> GetMostActiveUsersAsync(int limit = 10);
        Task<IEnumerable<object>> GetPopularPagesAsync(int limit = 10);
        Task<IEnumerable<object>> GetActivityTrendsAsync(int days = 30);
    }
} 