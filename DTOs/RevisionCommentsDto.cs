using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class RevisionCommentDto
    {
        [Required]
        public string Comment { get; set; }
    }
}
