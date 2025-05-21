namespace PersonaXFleet.DTOs
{
    public class UpdateRouteDto
    {

        public string Name { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public List<RouteUserDto> Users { get; set; }
    }
}
