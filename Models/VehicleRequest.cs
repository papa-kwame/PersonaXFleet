using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Models
{
public class VehicleRequest
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string RequestReason { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public VehicleRequestStatus Status { get; set; } = VehicleRequestStatus.Pending;
}

}