using PersonaXFleet.Models;

namespace PersonaXFleet.DTOs
{
    public class MaintenanceRequestDto
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string LicensePlate { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string AdminComments { get; set; } = string.Empty;
        public string? RequestedByUserId { get; set; }
        public string? RequestedByUserName { get; set; }

        public string RouteId { get; set; }
        public Router Route { get; set; }
        public string CurrentRouteId { get; set; }
        public Router CurrentRoute { get; set; }
        public string CurrentStage { get; set; } // "Comment", "Review", etc.

        public string Department { get; set; }

        public string RouteName { get; set; } // Add this line


    }
}