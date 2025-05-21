using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PersonaXFleet.Models
{
    public class Router
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public ICollection<UserRouteRole> UserRoles { get; set; } = new List<UserRouteRole>();

    }
}
