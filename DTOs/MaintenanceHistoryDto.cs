using System;

namespace PersonaXFleet.DTOs
{
    public class MaintenanceHistoryDto
    {
        public string HistoryId { get; set; }
        public string OriginalRequestId { get; set; }
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
        public decimal ? EstimatedCost { get; set; }
        public string AdminComments { get; set; }
        public string RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUser { get; set; }
        public string? ApprovalComments { get; set; }
    }
}