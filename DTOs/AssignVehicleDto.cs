using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class AssignVehicleDto
    {
        [Required]
        public string VehicleId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Reason { get; set; }

        public int EstimatedDuration { get; set; } = 30; 
    }
}
