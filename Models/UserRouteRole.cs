using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace PersonaXFleet.Models
{
    public class UserRouteRole
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string RouteId { get; set; }
        public string Role { get; set; }
        public ApplicationUser User { get; set; }

        [JsonIgnore]
        public Router Route { get; set; }
    }
}
