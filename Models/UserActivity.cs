using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        [Required]
        public string ?ActivityType { get; set; } // Login, Logout, Create, Update, Delete, View, etc.
        
        [Required]
        public string ?Module { get; set; } // Vehicles, Maintenance, Fuel, Users, etc.
        
        public string ?EntityType { get; set; } // Vehicle, MaintenanceRequest, FuelLog, etc.
        public string ?EntityId { get; set; } // ID of the specific entity
        
        public string  ?Description { get; set; } // Human-readable description
        public string  ?Details { get; set; } // JSON details of the action
        
        public string ? IpAddress { get; set; }
        public string ? UserAgent { get; set; }
        public string ?SessionId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? OldValues { get; set; } // JSON of previous state (for updates)
        public string? NewValues { get; set; } // JSON of new state (for updates)
        
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
        
        public int? DurationMs { get; set; } // How long the action took
        public string? PageUrl { get; set; } // Which page the action occurred on
    }
} 