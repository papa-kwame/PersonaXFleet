namespace PersonaXFleet.DTOs
{
    public class ApprovalDto
    {
        public string ApprovalId { get; set; }
        public string ApproverId { get; set; }
        public string ApproverName { get; set; }
        public int ApprovalLevel { get; set; }
        public string ApprovalStatus { get; set; }
        public string Comments { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
