using PersonaXFleet.Models;

namespace PersonaXFleet.DTOs
{
    // DTO
    public class MaintenanceProgressUpdateDto
    {
        public DateTime? ExpectedCompletionDate { get; set; }
        public string Comment { get; set; }
    }

}
