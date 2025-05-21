namespace PersonaXFleet.DTOs
{
    public class MaintenanceRequestApprovalDto
    {
        public string Comments { get; set; }
        public string ReviewerId { get; set; } // The ID of the user performing the action
    }
}
