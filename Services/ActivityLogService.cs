using PersonaXFleet.Data;
using PersonaXFleet.Models;

namespace PersonaXFleet.Services
{
    public class ActivityLogService
    {
        private readonly AuthDbContext _context;
        public ActivityLogService(AuthDbContext context) { _context = context; }

        public async Task LogAsync(string userId, string username, string action, string entity, string entityId, string details = null)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Username = username,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Details = details
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
