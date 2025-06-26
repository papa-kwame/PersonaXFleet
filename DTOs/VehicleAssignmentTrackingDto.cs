namespace PersonaXFleet.DTOs
{
    public class VehicleAssignmentTrackingDto
    {
        public string VehicleId { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string LicensePlate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
}
