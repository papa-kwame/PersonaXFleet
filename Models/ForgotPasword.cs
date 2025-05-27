using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class ForgotPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
