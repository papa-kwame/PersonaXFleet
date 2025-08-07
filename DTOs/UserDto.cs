namespace PersonaXFleet.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public bool IsLocked { get; set; }

        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
    }


}
