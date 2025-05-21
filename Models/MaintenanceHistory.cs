// MaintenanceHistory.cs
using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class MaintenanceHistory
    {
        public string HistoryId { get; set; } = Guid.NewGuid().ToString();
        public string OriginalRequestId { get; set; }
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string RequestedByUserId { get; set; }
        public ApplicationUser RequestedByUser { get; set; }
        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        public MaintenanceRequestType RequestType { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public MaintenanceRequestStatus Status { get; set; }
        public MaintenancePriority Priority { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string ? AdminComments { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ?ApprovedByUserId { get; set; }
        public string ? ApprovedByUser { get; set; }
        public string ? ApprovalComments { get; set; }
    }
}