namespace PersonaXFleet.DTOs
{
    public class CreateRouteDto
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public List<UserRoleDto> Users { get; set; } = new List<UserRoleDto>();
    }
}
