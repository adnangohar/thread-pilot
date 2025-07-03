using System;
using AutoMapper;
using Insurance.Contracts;
using Insurance.Domain.Entities;

namespace Insurance.Application.Mappings;

public class InsuranceMappingProfile : Profile
{
    public InsuranceMappingProfile()
    {
        CreateMap<CarInsurance, CarInsuranceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "CarInsurance"))
            .ForMember(dest => dest.Vehicle, opt => opt.Ignore());
            
        CreateMap<PetInsurance, PetInsuranceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "PetInsurance"));
            
        CreateMap<PersonalHealthInsurance, PersonalHealthInsuranceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "PersonalHealthInsurance"));
    }
}
