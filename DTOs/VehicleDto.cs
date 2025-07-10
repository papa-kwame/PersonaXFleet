using PersonaXFleet.Models;

namespace PersonaXFleet.DTOs
{
    public class VehicleDto
    {
        public string Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public string VehicleType { get; set; }
        public string Color { get; set; }
        public string Status { get; set; }
        public int CurrentMileage { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public decimal? EngineSize { get; set; }
        public int SeatingCapacity { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public int? ServiceInterval { get; set; }
        public DateTime? NextServiceDue { get; set; }
        public DateTime? RoadworthyExpiry { get; set; }
        public DateTime? RegistrationExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public string Notes { get; set; }
        public bool IsAssigned { get; set; }
    }
}
