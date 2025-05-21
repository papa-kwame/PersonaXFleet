using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class MaintenanceComment
    {
        [Key]
        public string CommentId { get; set; } = Guid.NewGuid().ToString();
        public string MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsInternal { get; set; }

    }
}
