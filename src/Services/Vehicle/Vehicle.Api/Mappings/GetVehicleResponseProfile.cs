using AutoMapper;
using Vehicle.Application.Common;
using Vehicle.Contracts;

namespace Vehicle.Api.Mappings
{
    public class GetVehicleResponseProfile : Profile
    {
        public GetVehicleResponseProfile()
        {
            CreateMap<VehicleResult, VehicleResponse>();
            CreateMap<VehicleResponse, VehicleResult>();
        }
    }
}
