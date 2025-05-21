// ApproveRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.DTOs
{
    public class ApproveRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Comments { get; set; }
    }
}