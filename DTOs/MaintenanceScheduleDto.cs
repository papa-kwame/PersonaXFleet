namespace PersonaXFleet.DTOs
{
    public class MaintenanceScheduleDto
    {
         public string AssignedMechanicId { get; set; }
        public string MaintenanceRequestId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
    }

}
