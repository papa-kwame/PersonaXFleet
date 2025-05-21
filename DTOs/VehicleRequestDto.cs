using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class VehicleRequestDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RequestReason { get; set; }

        [Required]
        public string Department { get; set; }
    }


}
