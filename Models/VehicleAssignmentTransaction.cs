using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class VehicleAssignmentTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string VehicleAssignmentRequestId { get; set; }  // ✅ renamed for clarity
        public VehicleAssignmentRequest Request { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Stage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comments { get; set; }
    }
}
