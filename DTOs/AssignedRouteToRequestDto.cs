using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class AssignRouteToRequestDto
    {
        [Required]
        public string RouteId { get; set; }
        [Required]
        public string MaintenanceRequestId { get; set; }
    }
}
