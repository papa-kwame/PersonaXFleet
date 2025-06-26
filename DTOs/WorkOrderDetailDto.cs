namespace PersonaXFleet.DTOs
{

    public class WorkOrderDetailDto : WorkOrderDto
    {
        public string PartsRequired { get; set; }
        public List<WorkOrderTaskDto> Tasks { get; set; }
    }

}
