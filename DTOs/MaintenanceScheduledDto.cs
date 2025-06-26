namespace PersonaXFleet.DTOs
{
    public class MaintenanceScheduledDto
    {
        public string Id { get; set; }
        public string MaintenanceRequestId { get; set; }
        public DateTime ? ScheduledDate { get; set; }
        public string Reason { get; set; }
        public string AssignedMechanicId { get; set; }
        public string AssignedMechanicName { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }

        public string RepairType { get; set; }
        public string VehicleId { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string LicensePlate { get; set; }
    }

}
