namespace PersonaXFleet.DTOs
{
    public class ApprovalRecordDto
    {
        public string Id { get; set; }
        public string RequiredRole { get; set; }
        public string ApproverUserId { get; set; }
        public string ApproverUserName { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Comments { get; set; }
        public int Order { get; set; }
    }
}
