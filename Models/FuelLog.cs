
using PersonaXFleet.Models.Enums;
using System;
    using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    namespace PersonaXFleet.Models
    {
        public class FuelLog
        {
            [Key]
            public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        public string UserId { get; set; }
            public ApplicationUser User { get; set; }

            [Required]
            [Range(0.1, double.MaxValue, ErrorMessage = "Fuel amount must be greater than zero.")]
            public decimal FuelAmount { get; set; } // in liters or gallons

            [Required]
            [Range(0.1, double.MaxValue, ErrorMessage = "Fuel cost must be greater than zero.")]
            public decimal Cost { get; set; } // in currency

            public DateTime Date { get; set; } = DateTime.UtcNow;


            public FuelStationType FuelStation { get; set; }


    }
    }


