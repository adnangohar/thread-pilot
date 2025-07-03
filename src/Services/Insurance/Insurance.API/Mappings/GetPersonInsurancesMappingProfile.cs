using AutoMapper;
using Insurance.Application.Common;

namespace Insurance.Api.Mappings;

public class GetPersonInsurancesMappingProfile : Profile
{
    public GetPersonInsurancesMappingProfile()
    {
        CreateMap<Contracts.InsuranceResponse, PersonInsurancesResult>();
    }
}
