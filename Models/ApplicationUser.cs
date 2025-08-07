using Microsoft.AspNetCore.Identity;

namespace PersonaXFleet.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Department { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = false; // false by default (not logged in)
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastActivityDate { get; set; } // Track last activity within the app
    }
}
