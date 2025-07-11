using System.Text.RegularExpressions;

namespace Vehicle.Core.ValueObjects;

public record RegistrationNumber
{
    private static readonly Regex ValidFormat = new(@"^[A-Z]{3}\d{3}$", RegexOptions.Compiled);

    public string Value { get; }

    public RegistrationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Registration number cannot be empty", nameof(value));

        value = value.ToUpperInvariant();

        if (!ValidFormat.IsMatch(value))
            throw new ArgumentException($"Invalid registration number format: {value}. Expected format: ABC123", nameof(value));

        Value = value;
    }

    public static implicit operator string(RegistrationNumber registrationNumber) => registrationNumber.Value;
    public override string ToString() => Value;
}
