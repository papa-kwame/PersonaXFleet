using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class AddCommentDto
    {
        [Required]
        public string Comment { get; set; }
        public bool IsInternal { get; set; } = false;
        public string AuthorId { get; set; } // this must be set

    }
}
