using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class CreateMaintenanceRequestDto
    {
        [Required]
        public MaintenanceRequestType RequestType { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Description { get; set; }

        [Required]
        public MaintenancePriority Priority { get; set; }

        public decimal? EstimatedCost { get; set; }
        public string? AdminComments { get; set; }

    }
}
