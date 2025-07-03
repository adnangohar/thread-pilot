using AutoMapper;

namespace Insurance.Application.Mappings;

public class InsuranceMappingProfile : Profile
{
    public InsuranceMappingProfile()
    {
        CreateMap<Domain.Entities.CarInsurance, Contracts.CarInsuranceResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "CarInsurance"))
            .ForMember(dest => dest.Vehicle, opt => opt.Ignore());
            
        CreateMap<Domain.Entities.PetInsurance, Contracts.PetInsuranceResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "PetInsurance"));
            
        CreateMap<Domain.Entities.PersonalHealthInsurance, Contracts.PersonalHealthInsuranceResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "PersonalHealthInsurance"));
    }
}
