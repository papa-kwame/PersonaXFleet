using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Models
{
    public class VehicleAssignmentRequestHistory
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public VehicleRequestStatus Status { get; set; }
        public string CurrentStage { get; set; }
        public List<VehicleAssignmentTransaction> Transactions { get; set; }
        public DateTime CompletionDate { get; set; }
        public Route CurrentRoute { get; set; }
    }
}
