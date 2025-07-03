using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public class CarInsurance : Insurance
{
    public string VehicleRegistrationNumber { get; private set; }
        
    public CarInsurance(PersonalIdentificationNumber owner, string vehicleRegistrationNumber) 
        : base(owner, 30m, InsuranceType.Car)
    {
        if (string.IsNullOrWhiteSpace(vehicleRegistrationNumber))
            throw new ArgumentException("Vehicle registration number cannot be empty", nameof(vehicleRegistrationNumber));
            
        VehicleRegistrationNumber = vehicleRegistrationNumber;
    }
}
