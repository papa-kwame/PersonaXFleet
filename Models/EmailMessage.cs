namespace PersonaXFleet.Models
{
    public class EmailMessage
    {
        public List<string> ToAddresses { get; set; } = new();
        public string Subject { get; set; }
        public string Body { get; set; }
    }

}
