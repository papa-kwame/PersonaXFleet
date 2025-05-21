using System.Text.Json.Serialization;

namespace PersonaXFleet.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}
