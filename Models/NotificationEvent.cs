using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Models
{
    public class NotificationEvent
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string RelatedEntityId { get; set; }
        public string ActionLink { get; set; }
    }

}
