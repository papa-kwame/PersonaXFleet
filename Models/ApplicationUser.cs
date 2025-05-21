using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Department { get; set;  }
    }
}
