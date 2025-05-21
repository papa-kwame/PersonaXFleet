// MaintenanceRequestDetailDto.cs
namespace PersonaXFleet.DTOs
{
    public class MaintenanceRequestDetailDto : MaintenanceRequestDto
    {

        public List<CommentDto> Comments { get; set; } = new();
        public List<StatusChangeDto> StatusHistory { get; set; } = new();
    }
}