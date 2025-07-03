using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public class PersonalHealthInsurance : Insurance
{
    public string CoverageLevel { get; private set; }
        
        public PersonalHealthInsurance(PersonalIdentificationNumber personalId, string coverageLevel = "Basic") 
            : base(personalId, 20m, InsuranceType.PersonalHealth)
        {
            CoverageLevel = coverageLevel;
        }
}
