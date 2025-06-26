namespace PersonaXFleet.Models
{
    public class PartUsed
    {
        public string Id { get; set; }
        public string InvoiceId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }

}
