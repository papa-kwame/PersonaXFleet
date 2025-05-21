using System.Text.Json.Serialization;

namespace PersonaXFleet.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenanceRequestType
    {
        RoutineMaintenance,
        Repair,
        Inspection,
        TireReplacement,
        BrakeService,
        OilChange,
        Upgrade,
        Emergency,
        Other
    }
}
