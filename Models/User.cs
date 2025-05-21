using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{

    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Department { get; set; }

        public bool IsActive { get; set; } = true;
        public string AvatarColor { get; set; } = "#4e73df";

        public ICollection<RouteUser> RouteUsers { get; set; }
    }
}
