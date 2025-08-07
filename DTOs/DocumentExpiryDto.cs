namespace PersonaXFleet.DTOs
{
    public class DocumentExpiryDto
    {
        public string VehicleId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public List<ExpiringDocument> ExpiringDocuments { get; set; } = new List<ExpiringDocument>();
    }

    public class ExpiringDocument
    {
        public string DocumentType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Status { get; set; } // "Expired", "Expiring Soon", "Warning"
    }
} 