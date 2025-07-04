using AutoMapper;
using Vehicle.Application.Common;

namespace Vehicle.Application.Mappings;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Domain.Entities.Vehicle, VehicleResult>()
            .ForMember(dest => dest.RegistrationNumber,
                opt => opt.MapFrom(src => src.RegistrationNumber));
    }
}