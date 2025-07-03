using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public abstract class Insurance
{
    public Guid Id { get; protected set; }
    public string PersonalId { get; protected set; }
    public decimal MonthlyCost { get; protected set; }
    public InsuranceType Type { get; protected set; }
    public string? VehicleRegistrationNumber { get; private set; } // For car insurance
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Insurance(PersonalIdentificationNumber personalId, decimal monthlyCost, InsuranceType type, string vehicleRegistrationNumber = null)
    {
        Id = Guid.NewGuid();
        PersonalId = personalId.Value ?? throw new ArgumentNullException(nameof(personalId));
        MonthlyCost = monthlyCost > 0 ? monthlyCost : throw new ArgumentException("Monthly cost must be positive", nameof(monthlyCost));
        Type = type;
        CreatedAt = DateTime.UtcNow;
        VehicleRegistrationNumber = vehicleRegistrationNumber;
    }
}
