using AutoMapper;
using Vehicle.Application.DTOs;
using Vehicle.Contracts;

namespace Vehicle.Application.Mappings
{
    public class GetVehicleResponseProfile : Profile
    {
        public GetVehicleResponseProfile()
        {
            CreateMap<VehicleDto, GetVehicleResponse>();
            CreateMap<GetVehicleResponse, VehicleDto>();
        }
    }
}
