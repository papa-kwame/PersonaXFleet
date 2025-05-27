using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.DTOs
{
    public class CreatePersonalMaintenanceRequestDto
    {

        public MaintenanceRequestType RequestType { get; set; }
        public string Description { get; set; }
        public MaintenancePriority Priority { get; set; }

    }

}
