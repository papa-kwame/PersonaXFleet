namespace PersonaXFleet.DTOs
{
    public class MaintenanceReviewDto
    {
        public string ReviewId { get; set; }
        public string RequestId { get; set; }
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public DateTime ReviewDate { get; set; }
        public string Comments { get; set; }
        public string RecommendedAction { get; set; }
        public bool IsApproved { get; set; }
    }
}
