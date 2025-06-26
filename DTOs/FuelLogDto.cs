using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.DTOs
{
    public class FuelLogDto
    {
       
        public decimal FuelAmount { get; set; }
        public decimal Cost { get; set; }
        public FuelStationType FuelStation { get; set; }

    }

}
