namespace PersonaXFleet.DTOs
{
    public class MaintenanceCommentDto
    {
        public string CommentId { get; set; }
        public string RequestId { get; set; }
        public string CommenterId { get; set; }
        public string CommenterName { get; set; }
        public DateTime CommentDate { get; set; }
        public string Text { get; set; }
        public bool IsInternalNote { get; set; }
    }
}
