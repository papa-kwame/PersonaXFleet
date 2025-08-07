using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Data;
using PersonaXFleet.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PersonaXFleet.Services
{
    public class UserActivityService : BackgroundService, IUserActivityService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserActivityService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
        private readonly int _inactiveMinutesThreshold = 30;

        public UserActivityService(IServiceProvider serviceProvider, ILogger<UserActivityService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckUserActivityAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking user activity");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task CheckUserActivityAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var cutoffDate = DateTime.UtcNow.AddMinutes(-_inactiveMinutesThreshold);
            
            var inactiveUsers = await userManager.Users
                .Where(u => u.IsActive && u.LastActivityDate < cutoffDate)
                .ToListAsync();

            foreach (var user in inactiveUsers)
            {
                user.IsActive = false;
                await userManager.UpdateAsync(user);
                
                // Log the inactivity
                await LogActivityAsync(user.Id, "SessionTimeout", "System", 
                    $"User marked as inactive after {_inactiveMinutesThreshold} minutes of no activity");
                
                _logger.LogInformation($"Set user {user.UserName} as inactive due to no recent activity");
            }

            if (inactiveUsers.Any())
            {
                _logger.LogInformation($"Set {inactiveUsers.Count} users as inactive due to inactivity");
            }
        }

        public async Task UpdateUserActivityAsync(string userId)
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                user.LastActivityDate = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }
        }

        // Advanced Activity Logging Methods
        public async Task LogActivityAsync(UserActivity activity)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            context.UserActivities.Add(activity);
            await context.SaveChangesAsync();
        }

        public async Task LogActivityAsync(string userId, string activityType, string module, string description = null, string entityType = null, string entityId = null, object details = null)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Module = module,
                Description = description,
                EntityType = entityType,
                EntityId = entityId,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                Timestamp = DateTime.UtcNow
            };

            await LogActivityAsync(activity);
        }

        public async Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var query = context.UserActivities
                .Include(ua => ua.User)
                .Where(ua => ua.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(ua => ua.Timestamp >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(ua => ua.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(ua => ua.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 50)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            return await context.UserActivities
                .Include(ua => ua.User)
                .OrderByDescending(ua => ua.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivity>> GetModuleActivitiesAsync(string module, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var query = context.UserActivities
                .Include(ua => ua.User)
                .Where(ua => ua.Module == module);

            if (fromDate.HasValue)
                query = query.Where(ua => ua.Timestamp >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(ua => ua.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(ua => ua.Timestamp)
                .ToListAsync();
        }

        public async Task<object> GetActivityStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var query = context.UserActivities.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ua => ua.Timestamp >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(ua => ua.Timestamp <= toDate.Value);

            var stats = new
            {
                TotalActivities = await query.CountAsync(),
                UniqueUsers = await query.Select(ua => ua.UserId).Distinct().CountAsync(),
                ActivitiesByModule = await query
                    .GroupBy(ua => ua.Module)
                    .Select(g => new { Module = g.Key, Count = g.Count() })
                    .ToListAsync(),
                ActivitiesByType = await query
                    .GroupBy(ua => ua.ActivityType)
                    .Select(g => new { ActivityType = g.Key, Count = g.Count() })
                    .ToListAsync(),
                RecentActivity = await query
                    .OrderByDescending(ua => ua.Timestamp)
                    .Take(10)
                    .Select(ua => new { 
                        ua.UserId, 
                        ua.ActivityType, 
                        ua.Module, 
                        ua.Timestamp 
                    })
                    .ToListAsync()
            };

            return stats;
        }

        public async Task<IEnumerable<object>> GetUserActivitySummaryAsync(string userId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var userActivities = await context.UserActivities
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            var summary = new List<object>
            {
                new { 
                    Metric = "Total Activities", 
                    Value = userActivities.Count 
                },
                new { 
                    Metric = "Most Active Module", 
                    Value = userActivities.GroupBy(ua => ua.Module)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "None"
                },
                new { 
                    Metric = "Most Common Action", 
                    Value = userActivities.GroupBy(ua => ua.ActivityType)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "None"
                },
                new { 
                    Metric = "Last Activity", 
                    Value = userActivities.Max(ua => ua.Timestamp)
                }
            };

            return summary;
        }

        public async Task<IEnumerable<object>> GetMostActiveUsersAsync(int limit = 10)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var userActivityStats = await context.UserActivities
                .GroupBy(ua => ua.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    ActivityCount = g.Count(),
                    LastActivity = g.Max(ua => ua.Timestamp),
                    MostActiveModule = g.GroupBy(ua => ua.Module)
                        .OrderByDescending(mg => mg.Count())
                        .First().Key
                })
                .OrderByDescending(u => u.ActivityCount)
                .Take(limit)
                .ToListAsync();

            // Get user details for the top users
            var userIds = userActivityStats.Select(u => u.UserId).ToList();
            var users = await context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            // Combine the data
            return userActivityStats.Select(stat => new
            {
                stat.UserId,
                UserName = users.FirstOrDefault(u => u.Id == stat.UserId)?.UserName ?? "Unknown User",
                UserEmail = users.FirstOrDefault(u => u.Id == stat.UserId)?.Email ?? "",
                stat.ActivityCount,
                stat.LastActivity,
                stat.MostActiveModule
            });
        }

        public async Task<IEnumerable<object>> GetPopularPagesAsync(int limit = 10)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            return await context.UserActivities
                .Where(ua => ua.PageUrl != null)
                .GroupBy(ua => ua.PageUrl)
                .Select(g => new
                {
                    PageUrl = g.Key,
                    VisitCount = g.Count(),
                    UniqueUsers = g.Select(ua => ua.UserId).Distinct().Count()
                })
                .OrderByDescending(p => p.VisitCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetActivityTrendsAsync(int days = 30)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            return await context.UserActivities
                .Where(ua => ua.Timestamp >= startDate)
                .GroupBy(ua => ua.Timestamp.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    ActivityCount = g.Count(),
                    UniqueUsers = g.Select(ua => ua.UserId).Distinct().Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }
    }
} 