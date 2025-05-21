using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class MaintenanceApproval
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MaintenanceRequest")]
        public string MaintenanceRequestId { get; set; }

        public DateTime ApprovedAt { get; set; }

        public MaintenanceRequest MaintenanceRequest { get; set; }

        public string ApprovalComments { get; set; }
    }
}
