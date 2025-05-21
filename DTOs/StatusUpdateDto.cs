using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    using PersonaXFleet.Models.Enums;
    using System.ComponentModel.DataAnnotations;

    namespace VehicleAPI.Models
    {
        public class StatusUpdateDTO
        {
            [Required]
            public VehicleRequestStatus Status { get; set; }
        }
    }

}
