using Microsoft.AspNetCore.Identity;
using PersonaXFleet.Models.Enums;

namespace PersonaXFleet.Models
{

    public class ApprovalRecord
    {
        public string Id { get; set; }
        public string MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; }
        public ApprovalRoleType RequiredRole { get; set; }
        public string ApproverUserId { get; set; }
        public ApplicationUser ApproverUser { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Comments { get; set; }
        public int Order { get; set; } // To define the sequence of approvals
    }

}
