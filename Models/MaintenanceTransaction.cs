using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonaXFleet.Models
{
    public class MaintenanceTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public MaintenanceRequest MaintenanceRequest { get; set; }
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Action { get; set; } 
        public string Comments { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsCompleted { get; set; }


    }
}
