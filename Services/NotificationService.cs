using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonaXFleet.Data;
using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonaXFleet.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackgroundTaskQueue _taskQueue;

        public NotificationService(AuthDbContext context, UserManager<ApplicationUser> userManager, IBackgroundTaskQueue taskQueue  )
        {
            _context = context;
            _userManager = userManager;
            _taskQueue = taskQueue;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, string relatedEntityId, string actionLink = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var notification = new Notification
            {
                UserId = userId,
                Username = user.UserName,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                ActionLink = actionLink
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }
    }
}
