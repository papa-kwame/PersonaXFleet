using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Department { get; set; }
        public bool MustChangePassword { get; set; } = false;
    }

}
