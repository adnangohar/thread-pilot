using System.Text.RegularExpressions;

namespace Vehicle.Api.Validation;

public static class RegistrationNumberValidator
{
    public static string? Validate(string? registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            return "registrationNumber is required.";
        if (registrationNumber.Length < 2 || registrationNumber.Length > 20)
            return "registrationNumber must be between 2 and 20 characters.";
        if (!Regex.IsMatch(registrationNumber, "^[a-zA-Z0-9]+$"))
            return "registrationNumber must contain only letters and numbers.";
        return null;
    }
}
