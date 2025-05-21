using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class ProcessRequestDto
    {
        [Required]
        public string RequestId { get; set; }
        [Required]
        public bool Approve { get; set; }
        public string AdminNotes { get; set; }
    }
}
