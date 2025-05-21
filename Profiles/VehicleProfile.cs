using AutoMapper;
using PersonaXFleet.DTOs;
using PersonaXFleet.Models;

namespace PersonaXFleet.Profiles
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<VehicleDto, Vehicle>();
        }
    }
}