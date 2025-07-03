using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public class PetInsurance : Insurance
{
    public string PetName { get; private set; }
    public string PetType { get; private set; }
        
    public PetInsurance(PersonalIdentificationNumber owner, string petName = "Unknown", string petType = "Unknown") 
        : base(owner, 10m, InsuranceType.Pet)
    {
        PetName = petName;
        PetType = petType;
    }
}
