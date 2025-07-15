using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }      

        public  string Username { get; set; }
        public string Title { get; set; }    
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string RelatedEntityId { get; set; } 
        public string ? ActionLink { get; set; }    
    }
}
