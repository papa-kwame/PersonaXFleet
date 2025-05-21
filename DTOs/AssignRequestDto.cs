// AssignRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class AssignRequestDto
    {
        [Required]
        public string UserId { get; set; }

        public string Comment { get; set; }
    }
}