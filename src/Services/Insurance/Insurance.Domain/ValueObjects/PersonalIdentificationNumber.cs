using System.Text.RegularExpressions;

namespace Insurance.Domain.ValueObjects;

public record PersonalIdentificationNumber
{
    // TODO - Use a library for validating Swedish personal identification numbers
    private static readonly Regex ValidFormat = new(@"^\d{9}$", RegexOptions.Compiled);
        
        public string Value { get; }
        
        public PersonalIdentificationNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Personal identification number cannot be empty", nameof(value));
                
            value = value.Trim();
            
            if (!ValidFormat.IsMatch(value))
                throw new ArgumentException($"Invalid personal identification number format: {value}. Expected 9 digits", nameof(value));
                
            Value = value;
        }
        
        public static implicit operator string(PersonalIdentificationNumber pin) => pin.Value;
        public override string ToString() => Value;
}
