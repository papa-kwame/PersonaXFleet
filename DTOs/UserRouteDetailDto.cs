
namespace PersonaXFleet.DTOs
{
    public class UserRouteDetailDto
    {
        public string RouteId { get; set; }
        public string RouteName { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public string MyRole { get; set; }
        public List<UserRoleDto> OtherUsers { get; set; }
    }
}
