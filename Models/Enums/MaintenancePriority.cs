using System.Text.Json.Serialization;

namespace PersonaXFleet.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenancePriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
