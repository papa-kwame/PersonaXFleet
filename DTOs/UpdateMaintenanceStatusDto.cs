using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class UpdateMaintenanceStatusDto
    {
        [Required]
        public MaintenanceRequestStatus Status { get; set; }

        [StringLength(1000)]
        public string? AdminComments { get; set; }
    }
}
