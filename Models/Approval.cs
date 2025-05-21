using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class Approval
    {
        [Key]
        public string ApprovalId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string MaintenanceRequestId { get; set; }

        [Required]
        public int ApprovalLevel { get; set; }

        public string ApproverId { get; set; }

        [Required]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public string Comments { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Navigation properties
        public MaintenanceRequest MaintenanceRequest { get; set; }
        public ApplicationUser Approver { get; set; }
    }
}
