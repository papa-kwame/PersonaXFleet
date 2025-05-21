using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.DTOs
{
    public class CreatePersonalMaintenanceRequestDto
    {
        public string Department { get; set; }
        public MaintenanceRequestType RequestType { get; set; }
        public string Description { get; set; }
        public MaintenancePriority Priority { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? AdminComments { get; set; }
    }

}
