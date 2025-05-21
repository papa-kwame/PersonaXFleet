using PersonaXFleet.DTOs;

namespace PersonaXFleet.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto> GetVehicleByIdAsync(string id);
        Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicleDto);
        Task UpdateVehicleAsync(string id, VehicleDto vehicleDto);
        Task DeleteVehicleAsync(string id);
    }
}
