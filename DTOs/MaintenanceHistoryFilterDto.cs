// MaintenanceHistoryFilterDto.cs
namespace PersonaXFleet.DTOs
{
    public class MaintenanceHistoryFilterDto
    {
        public string? VehicleId { get; set; }
        public string? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }
}