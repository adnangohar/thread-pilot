using Insurance.Domain.Enums;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Entities;

public class Insurance
{
    public Guid Id { get; set; }
    public string PersonalId { get; set; }
    public decimal MonthlyCost { get; set; }
    public InsuranceType Type { get; set; }
    public string? VehicleRegistrationNumber { get; private set; } // For car insurance
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Insurance() { } // EF Core
    
    public Insurance(PersonalIdentificationNumber personalId, decimal monthlyCost, InsuranceType type, string vehicleRegistrationNumber = null)
    {
        Id = Guid.NewGuid();
        PersonalId = personalId.Value ?? throw new ArgumentNullException(nameof(personalId));
        MonthlyCost = monthlyCost > 0 ? monthlyCost : throw new ArgumentException("Monthly cost must be positive", nameof(monthlyCost));
        Type = type;
        CreatedAt = DateTime.UtcNow;
        VehicleRegistrationNumber = vehicleRegistrationNumber;
    }
}
