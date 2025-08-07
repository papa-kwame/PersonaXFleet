﻿using PersonaXFleet.DTOs;
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
    if (vehicleDto == null)
    {
        throw new ArgumentNullException(nameof(vehicleDto), "Vehicle data is required.");
    }

    // Parse EngineSize safely
    decimal? engineSize = null;
    if (vehicleDto.EngineSize != null && decimal.TryParse(vehicleDto.EngineSize.ToString(), out var parsedEngineSize))
    {
        engineSize = parsedEngineSize;
    }

    var vehicle = new Vehicle
    {
        Id = vehicleDto.Id,
        Make = vehicleDto.Make,
        Model = vehicleDto.Model,
        Year = vehicleDto.Year,
        LicensePlate = vehicleDto.LicensePlate,
        VIN = vehicleDto.VIN,
        VehicleType = vehicleDto.VehicleType,
        Color = vehicleDto.Color,
        Status = VehicleStatus.Available, // Default status or other logic
        CurrentMileage = vehicleDto.CurrentMileage,
        FuelType = Enum.Parse<FuelType>(vehicleDto.FuelType),
        Transmission = Enum.Parse<TransmissionType>(vehicleDto.Transmission),
        EngineSize = engineSize,
        SeatingCapacity = vehicleDto.SeatingCapacity,
        PurchaseDate = vehicleDto.PurchaseDate,
        PurchasePrice = vehicleDto.PurchasePrice,
        LastServiceDate = vehicleDto.LastServiceDate,
        ServiceInterval = vehicleDto.ServiceInterval,
        NextServiceDue = vehicleDto.NextServiceDue,
        RoadworthyExpiry = vehicleDto.RoadworthyExpiry,
        RegistrationExpiry = vehicleDto.RegistrationExpiry,
        InsuranceExpiry = vehicleDto.InsuranceExpiry,
        Notes = vehicleDto.Notes,
       
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
                Notes = vehicle.Notes,
                IsAssigned = !string.IsNullOrEmpty(vehicle.UserId)
            };
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesWithDueDatesAsync(int daysThreshold)
        {
            var today = DateTime.Today;
            var maxDate = today.AddDays(daysThreshold);

            var vehicles = await _context.Vehicles
                .Where(v =>
                    (v.NextServiceDue.HasValue && v.NextServiceDue.Value.Date <= maxDate) ||
                    (v.RoadworthyExpiry.HasValue && v.RoadworthyExpiry.Value.Date <= maxDate) ||
                    (v.RegistrationExpiry.HasValue && v.RegistrationExpiry.Value.Date <= maxDate) ||
                    (v.InsuranceExpiry.HasValue && v.InsuranceExpiry.Value.Date <= maxDate))
                .ToListAsync();

            return vehicles.Select(MapToDto);
        }

        public async Task<IEnumerable<DocumentExpiryDto>> GetDocumentExpiryDetailsAsync(int daysThreshold)
        {
            var today = DateTime.Today;
            var maxDate = today.AddDays(daysThreshold);

            var vehicles = await _context.Vehicles
                .Where(v =>
                    (v.NextServiceDue.HasValue && v.NextServiceDue.Value.Date <= maxDate) ||
                    (v.RoadworthyExpiry.HasValue && v.RoadworthyExpiry.Value.Date <= maxDate) ||
                    (v.RegistrationExpiry.HasValue && v.RegistrationExpiry.Value.Date <= maxDate) ||
                    (v.InsuranceExpiry.HasValue && v.InsuranceExpiry.Value.Date <= maxDate))
                .ToListAsync();

            var result = new List<DocumentExpiryDto>();

            foreach (var vehicle in vehicles)
            {
                var expiryDto = new DocumentExpiryDto
                {
                    VehicleId = vehicle.Id,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    LicensePlate = vehicle.LicensePlate,
                    ExpiringDocuments = new List<ExpiringDocument>()
                };

                // Check each document type
                if (vehicle.NextServiceDue.HasValue && vehicle.NextServiceDue.Value.Date <= maxDate)
                {
                    var daysUntilExpiry = (vehicle.NextServiceDue.Value.Date - today).Days;
                    expiryDto.ExpiringDocuments.Add(new ExpiringDocument
                    {
                        DocumentType = "Service Due",
                        ExpiryDate = vehicle.NextServiceDue.Value,
                        DaysUntilExpiry = daysUntilExpiry,
                        Status = GetExpiryStatus(daysUntilExpiry)
                    });
                }

                if (vehicle.RoadworthyExpiry.HasValue && vehicle.RoadworthyExpiry.Value.Date <= maxDate)
                {
                    var daysUntilExpiry = (vehicle.RoadworthyExpiry.Value.Date - today).Days;
                    expiryDto.ExpiringDocuments.Add(new ExpiringDocument
                    {
                        DocumentType = "Roadworthy Certificate",
                        ExpiryDate = vehicle.RoadworthyExpiry.Value,
                        DaysUntilExpiry = daysUntilExpiry,
                        Status = GetExpiryStatus(daysUntilExpiry)
                    });
                }

                if (vehicle.RegistrationExpiry.HasValue && vehicle.RegistrationExpiry.Value.Date <= maxDate)
                {
                    var daysUntilExpiry = (vehicle.RegistrationExpiry.Value.Date - today).Days;
                    expiryDto.ExpiringDocuments.Add(new ExpiringDocument
                    {
                        DocumentType = "Registration",
                        ExpiryDate = vehicle.RegistrationExpiry.Value,
                        DaysUntilExpiry = daysUntilExpiry,
                        Status = GetExpiryStatus(daysUntilExpiry)
                    });
                }

                if (vehicle.InsuranceExpiry.HasValue && vehicle.InsuranceExpiry.Value.Date <= maxDate)
                {
                    var daysUntilExpiry = (vehicle.InsuranceExpiry.Value.Date - today).Days;
                    expiryDto.ExpiringDocuments.Add(new ExpiringDocument
                    {
                        DocumentType = "Insurance",
                        ExpiryDate = vehicle.InsuranceExpiry.Value,
                        DaysUntilExpiry = daysUntilExpiry,
                        Status = GetExpiryStatus(daysUntilExpiry)
                    });
                }

                if (expiryDto.ExpiringDocuments.Any())
                {
                    result.Add(expiryDto);
                }
            }

            return result.OrderBy(v => v.ExpiringDocuments.Min(d => d.DaysUntilExpiry));
        }

        private string GetExpiryStatus(int daysUntilExpiry)
        {
            if (daysUntilExpiry < 0)
                return "Expired";
            else if (daysUntilExpiry <= 7)
                return "Critical";
            else if (daysUntilExpiry <= 30)
                return "Warning";
            else
                return "Expiring Soon";
        }

        Task IVehicleService.DeleteVehicleAsync(string id)
        {
            return DeleteVehicleAsync(id);
        }
    }

}
