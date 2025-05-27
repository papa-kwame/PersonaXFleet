using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class VehicleRequestDto
    {
        public string UserId { get; set; }
        public string VehicleId { get; set; }
        public string RequestReason { get; set; }
    }



}
