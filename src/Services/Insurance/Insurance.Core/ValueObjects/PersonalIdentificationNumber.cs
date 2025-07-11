namespace Insurance.Core.ValueObjects;

public record PersonalIdentificationNumber
{      
        public string Value { get; }
        
        public PersonalIdentificationNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Personal identification number cannot be empty", nameof(value));
                
            Value = value;
        }
        
        public static implicit operator string(PersonalIdentificationNumber pin) => pin.Value;
        public override string ToString() => Value;
}
