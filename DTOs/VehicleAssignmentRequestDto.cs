namespace PersonaXFleet.DTOs
{
    public class VehicleAssignmentRequestDto
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string LicensePlate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public string ExpectedDuration { get; set; }
        public string Status { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string AdminNotes { get; set; }
        public string AdditionalNotes { get; set; }
    }
}
