using PersonaXFleet.Models;
using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message,
            NotificationType type, string relatedEntityId, string actionLink = null);
        Task MarkAsReadAsync(Guid notificationId);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);

        Task SoftDeleteAsync(Guid id);
    }
}
