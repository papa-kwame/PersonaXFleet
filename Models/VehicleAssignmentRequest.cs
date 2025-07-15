using PersonaXFleet.Models.Enums;
using PersonaXFleet.Models;

public class VehicleAssignmentRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } 
    public string RequestReason { get; set; }
    public DateTime RequestDate { get; set; }

    public string CurrentStage { get; set; } = "Comment";
    public VehicleRequestStatus Status { get; set; } = VehicleRequestStatus.Pending;
    public string CurrentRouteId { get; set; }
    public Router CurrentRoute { get; set; }
    public ICollection<VehicleAssignmentTransaction> Transactions { get; set; }
}
