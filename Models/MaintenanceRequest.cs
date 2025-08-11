using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public string MaintenanceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        [Required]
        public string RequestedByUserId { get; set; }
        public ApplicationUser RequestedByUser { get; set; }

        [Required]
        public MaintenanceRequestType RequestType { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        [Required]
        public MaintenanceRequestStatus Status { get; set; } = MaintenanceRequestStatus.Pending;

        [Required]
        public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;

        public decimal? EstimatedCost { get; set; }

        [StringLength(1000)]
        public string ? AdminComments { get; set; }
        public string? CurrentRouteId { get; set; }
        public Router? CurrentRoute { get; set; }

        public string CurrentStage { get; set; } // "Comment", "Review", etc.

        public int? TotatalCostofRepair { get; set; }

        public string Department { get; set; }
        public ICollection<MaintenanceTransaction> Transactions { get; set; }


    }
}
