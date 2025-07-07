using AutoMapper;

namespace Insurance.Application.Mappings;

public class InsuranceMappingProfile : Profile
{
    public InsuranceMappingProfile()
    {
        CreateMap<Domain.Entities.Insurance, Contracts.InsuranceResponse>();
    }
}
