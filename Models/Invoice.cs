namespace PersonaXFleet.Models
{
   public class Invoice
{
    public string Id { get; set; }
    public string MaintenanceScheduleId { get; set; }
    public decimal LaborHours { get; set; }
    public decimal TotalCost { get; set; }
    public string SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
}

}
