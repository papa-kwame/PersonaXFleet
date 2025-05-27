using PersonaXFleet.DTOs;
using PersonaXFleet.Models.Enums;
using PersonaXFleet.Models;
using System;
using PersonaXFleet.Data;
using Microsoft.EntityFrameworkCore;

namespace PersonaXFleet.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AuthDbContext _context;

        public VehicleService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .Select(v => MapToDto(v))
                .ToListAsync();
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            return vehicle != null ? MapToDto(vehicle) : null;
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicleDto)
        {
            var vehicle = new Vehicle
            {
                // Ensure all required fields are mapped
                Make = vehicleDto.Make,
                Model = vehicleDto.Model,
                Year = vehicleDto.Year,
                LicensePlate = vehicleDto.LicensePlate,
                VIN = vehicleDto.VIN,
                VehicleType = vehicleDto.VehicleType,
                Color = vehicleDto.Color,
                Status = Enum.Parse<VehicleStatus>(vehicleDto.Status),
                CurrentMileage = vehicleDto.CurrentMileage,
                FuelType = Enum.Parse<FuelType>(vehicleDto.FuelType),
                Transmission = Enum.Parse<TransmissionType>(vehicleDto.Transmission),
                EngineSize = vehicleDto.EngineSize, // Now matches decimal type
                SeatingCapacity = vehicleDto.SeatingCapacity,
                PurchaseDate = vehicleDto.PurchaseDate,
                PurchasePrice = vehicleDto.PurchasePrice,
                LastServiceDate = vehicleDto.LastServiceDate,
                ServiceInterval = vehicleDto.ServiceInterval,
                NextServiceDue = vehicleDto.NextServiceDue,
                RoadworthyExpiry = vehicleDto.RoadworthyExpiry,
                RegistrationExpiry = vehicleDto.RegistrationExpiry,
                InsuranceExpiry = vehicleDto.InsuranceExpiry,
                Notes = vehicleDto.Notes
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return MapToDto(vehicle);
        }

        public async Task UpdateVehicleAsync(string id, VehicleDto vehicleDto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {id} not found.");
            }

            vehicle.Make = vehicleDto.Make;
            vehicle.Model = vehicleDto.Model;
            vehicle.Year = vehicleDto.Year;
            vehicle.LicensePlate = vehicleDto.LicensePlate;
            vehicle.VIN = vehicleDto.VIN;
            vehicle.VehicleType = vehicleDto.VehicleType;
            vehicle.Color = vehicleDto.Color;
            vehicle.Status = Enum.Parse<VehicleStatus>(vehicleDto.Status);
            vehicle.CurrentMileage = vehicleDto.CurrentMileage;
            vehicle.FuelType = Enum.Parse<FuelType>(vehicleDto.FuelType);
            vehicle.Transmission = Enum.Parse<TransmissionType>(vehicleDto.Transmission);
            vehicle.EngineSize = vehicleDto.EngineSize;
            vehicle.SeatingCapacity = vehicleDto.SeatingCapacity;
            vehicle.PurchaseDate = vehicleDto.PurchaseDate;
            vehicle.PurchasePrice = vehicleDto.PurchasePrice;
            vehicle.LastServiceDate = vehicleDto.LastServiceDate;
            vehicle.ServiceInterval = vehicleDto.ServiceInterval;
            vehicle.NextServiceDue = vehicleDto.NextServiceDue;
            vehicle.RoadworthyExpiry = vehicleDto.RoadworthyExpiry;
            vehicle.RegistrationExpiry = vehicleDto.RegistrationExpiry;
            vehicle.InsuranceExpiry = vehicleDto.InsuranceExpiry;
            vehicle.Notes = vehicleDto.Notes;

            await _context.SaveChangesAsync();
        }

public async Task<bool> DeleteVehicleAsync(string id)
{
    var vehicle = await _context.Vehicles.FindAsync(id);

    if (vehicle == null)
        return false;

    // Check if the vehicle is currently assigned to a user
    if (!string.IsNullOrEmpty(vehicle.UserId))
    {
        // Vehicle is assigned; do not delete
        return false;
    }

    _context.Vehicles.Remove(vehicle);
    await _context.SaveChangesAsync();
    return true;
}


        private static VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                VehicleType = vehicle.VehicleType,
                Color = vehicle.Color,
                Status = vehicle.Status.ToString(),
                CurrentMileage = vehicle.CurrentMileage,
                FuelType = vehicle.FuelType.ToString(),
                Transmission = vehicle.Transmission.ToString(),
                EngineSize = vehicle.EngineSize,
                SeatingCapacity = vehicle.SeatingCapacity,
                PurchaseDate = vehicle.PurchaseDate,
                PurchasePrice = vehicle.PurchasePrice,
                LastServiceDate = vehicle.LastServiceDate,
                ServiceInterval = vehicle.ServiceInterval,
                NextServiceDue = vehicle.NextServiceDue,
                RoadworthyExpiry = vehicle.RoadworthyExpiry,
                RegistrationExpiry = vehicle.RegistrationExpiry,
                InsuranceExpiry = vehicle.InsuranceExpiry,
                Notes = vehicle.Notes
            };
        }

        Task IVehicleService.DeleteVehicleAsync(string id)
        {
            return DeleteVehicleAsync(id);
        }
    }

}
