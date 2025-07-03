using System;
using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public abstract class Insurance
{
    public Guid Id { get; protected set; }
    public PersonalIdentificationNumber Owner { get; protected set; }
    public decimal MonthlyCost { get; protected set; }
    public InsuranceType Type { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
        
    protected Insurance(PersonalIdentificationNumber owner, decimal monthlyCost, InsuranceType type)
    {
        Id = Guid.NewGuid();
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        MonthlyCost = monthlyCost > 0 ? monthlyCost : throw new ArgumentException("Monthly cost must be positive", nameof(monthlyCost));
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }
}
