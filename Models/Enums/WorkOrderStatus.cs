namespace PersonaXFleet.Models.Enums
{
    public enum WorkOrderStatus
    {
        Pending,    // Created but unassigned
        Assigned,   // Technician assigned but not started
        InProgress, // Work started
        OnHold,     // Paused
        Completed,
        Cancelled
    }
}
