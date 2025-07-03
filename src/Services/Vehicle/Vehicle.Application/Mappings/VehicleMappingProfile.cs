using AutoMapper;
using Vehicle.Application.DTOs;

namespace Vehicle.Application.Mappings;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Domain.Entities.Vehicle, VehicleDto>()
            .ForMember(dest => dest.RegistrationNumber,
                opt => opt.MapFrom(src => src.RegistrationNumber.Value));
    }
}