using AutoMapper;
using Insurance.Core.Common;

namespace Insurance.Core.Mappings;

public class InsuranceMappingProfile : Profile
{
    public InsuranceMappingProfile()
    {
        CreateMap<Entities.Insurance, InsuranceResponse>();
    }
}
