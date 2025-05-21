using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PersonaXFleet.Models
{
    public class RouteUser
    {
        public string RouteId { get; set; }
        public Route Route { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
    }
}
