using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic.FileIO;
    using PersonaXFleet.Models.Enums;
    using System.ComponentModel.DataAnnotations;

    namespace PersonaXFleet.Models
    {
        public class Vehicle
        {
            [Key]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [Required]
            [StringLength(50)]
            public string Make { get; set; }

            [Required]
            [StringLength(50)]
            public string Model { get; set; }

            [Required]
            public int Year { get; set; }

            [Required]
            [StringLength(20)]
            public string LicensePlate { get; set; }

            [StringLength(50)]
            public string VIN { get; set; }

            [StringLength(50)]
            public string VehicleType { get; set; }

            [StringLength(30)]
            public string Color { get; set; }

            public VehicleStatus Status { get; set; }

            public int CurrentMileage { get; set; }

            public FuelType FuelType { get; set; }

            public TransmissionType Transmission { get; set; }

            public decimal? EngineSize { get; set; } // in cc

            public int SeatingCapacity { get; set; }

            public DateTime? PurchaseDate { get; set; }

            public decimal? PurchasePrice { get; set; }

            public DateTime? LastServiceDate { get; set; }

            public int? ServiceInterval { get; set; } // in miles

            public DateTime? NextServiceDue { get; set; }

            public DateTime? RoadworthyExpiry { get; set; }

            public DateTime? RegistrationExpiry { get; set; }

            public DateTime? InsuranceExpiry { get; set; }

            public string Notes { get; set; }

        // Add these properties for user assignment
            public string UserId { get; set; }
            public ApplicationUser User { get; set; }

        public ICollection<VehicleAssignmentHistory> AssignmentHistory { get; set; }



    }
}
