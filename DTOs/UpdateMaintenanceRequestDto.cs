using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class UpdateMaintenanceRequestDto
    {
        [Required]
        public MaintenanceRequestStatus Status { get; set; }

        [StringLength(1000)]
        public string AdminComments { get; set; }
    }
}
