using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class VehicleAssignmentHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? UnassignmentDate { get; set; }
    }
}
