namespace PersonaXFleet.DTOs
{
    public class WorkOrderDto
    {
        public string Id { get; set; }
        public string MaintenanceRequestId { get; set; }
        public string VehicleId { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string LicensePlate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string AssignedMechanicId { get; set; }
        public string AssignedMechanicName { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public string WorkDetails { get; set; }

        public bool IsAssigned { get; set; }
    
        public string PartsRequired { get; set; }
    }
}
