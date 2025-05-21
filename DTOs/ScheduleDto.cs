namespace PersonaXFleet.DTOs
{
    public class ScheduleDto
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public VehicleDto Vehicle { get; set; }
        public string MaintenanceType { get; set; }
        public DateTime LastServiceDate { get; set; }
        public int LastServiceMileage { get; set; }
        public DateTime NextServiceDate { get; set; }
        public int NextServiceMileage { get; set; }
        public int CurrentMileage { get; set; }
        public string Status { get; set; }
    }
}
