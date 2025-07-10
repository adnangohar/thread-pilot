using AutoMapper;
using Vehicle.Core.Common;

namespace Vehicle.Core.Mappings;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Entities.Vehicle, VehicleResult>()
            .ForMember(dest => dest.RegistrationNumber,
                opt => opt.MapFrom(src => src.RegistrationNumber));
    }
}