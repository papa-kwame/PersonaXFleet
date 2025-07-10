namespace PersonaXFleet.Models
{
    public class MaintenanceSchedule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public DateTime CompletedDate { get; set; }
        public string Reason { get; set; }
        public string  ? AssignedMechanicId { get; set; }
        public ApplicationUser ? AssignedMechanic { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }

        // New property
        public DateTime DateCreated { get; set; }
    }

}
