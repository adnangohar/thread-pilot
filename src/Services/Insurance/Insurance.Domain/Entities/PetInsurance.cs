using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public class PetInsurance : Insurance
{
    public string PetName { get; private set; }
    public string PetType { get; private set; }
        
    public PetInsurance(PersonalIdentificationNumber personalId, string petName = "Unknown", string petType = "Unknown") 
        : base(personalId, 10m, InsuranceType.Pet)
    {
        PetName = petName;
        PetType = petType;
    }
}
