using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class VehicleRequestUpdateDTO
    {
        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

}
