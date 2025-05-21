namespace PersonaXFleet.DTOs
{
    public class CommentDto
    {
        public string Id { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; }
    }
}
