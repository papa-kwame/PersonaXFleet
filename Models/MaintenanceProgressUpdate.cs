namespace PersonaXFleet.Models
{
    public class MaintenanceProgressUpdate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MaintenanceScheduleId { get; set; }
        public MaintenanceSchedule MaintenanceSchedule { get; set; }

        public string MechanicId { get; set; }
        public ApplicationUser Mechanic { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
