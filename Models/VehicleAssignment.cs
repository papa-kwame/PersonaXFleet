using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class VehicleAssignment
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EndReason { get; set; }
        public string AssignmentRequestId { get; set; }
        public string Department { get; set; }
    }

}
