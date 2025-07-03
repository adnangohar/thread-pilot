using FluentValidation;
using Insurance.Contracts;
using System.Text.RegularExpressions;

namespace Insurance.Api.Validation;

public class GetPersonInsurancesRequestValidator : AbstractValidator<GetPersonInsurancesRequest>
{
    public GetPersonInsurancesRequestValidator()
    {
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty()
            .WithMessage("Personal identification number is required.")
            .Must(IsValidSwedishPersonalNumber)
            .WithMessage("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
    }

    private bool IsValidSwedishPersonalNumber(string personalNumber)
    {
        if (string.IsNullOrWhiteSpace(personalNumber))
            return false;

        // Remove any whitespace
        personalNumber = personalNumber.Trim();

        // Try to parse different formats
        var parsedNumber = ParsePersonalNumber(personalNumber);
        if (parsedNumber == null)
            return false;

        // Validate the date part
        if (!IsValidDate(parsedNumber.Year, parsedNumber.Month, parsedNumber.Day))
            return false;

        // Validate using Luhn algorithm
        return IsValidLuhn(parsedNumber.TenDigitNumber);
    }

    private PersonalNumberParts? ParsePersonalNumber(string personalNumber)
    {
        // Pattern for YYYYMMDD-BBBC or YYYYMMDD+BBBC (12 digits + delimiter)
        var fullYearPattern = @"^(\d{4})(\d{2})(\d{2})[-+](\d{3})(\d{1})$";
        var fullYearMatch = Regex.Match(personalNumber, fullYearPattern);

        if (fullYearMatch.Success)
        {
            var year = int.Parse(fullYearMatch.Groups[1].Value);
            var month = int.Parse(fullYearMatch.Groups[2].Value);
            var day = int.Parse(fullYearMatch.Groups[3].Value);
            var birthNumber = fullYearMatch.Groups[4].Value;
            var checksum = fullYearMatch.Groups[5].Value;

            // Create 10-digit version for Luhn validation (YY format)
            var shortYear = year % 100;
            var tenDigitNumber = $"{shortYear:D2}{month:D2}{day:D2}{birthNumber}{checksum}";

            return new PersonalNumberParts(year, month, day, birthNumber, checksum, tenDigitNumber);
        }

        // Pattern for YYMMDD-BBBC or YYMMDD+BBBC (10 digits + delimiter)
        var shortYearPattern = @"^(\d{2})(\d{2})(\d{2})[-+](\d{3})(\d{1})$";
        var shortYearMatch = Regex.Match(personalNumber, shortYearPattern);

        if (shortYearMatch.Success)
        {
            var yearTwoDigit = int.Parse(shortYearMatch.Groups[1].Value);
            var month = int.Parse(shortYearMatch.Groups[2].Value);
            var day = int.Parse(shortYearMatch.Groups[3].Value);
            var birthNumber = shortYearMatch.Groups[4].Value;
            var checksum = shortYearMatch.Groups[5].Value;

            // Determine full year (assume 19xx for years >= 50, 20xx for years < 50)
            var currentYear = DateTime.Now.Year;
            var currentCentury = currentYear / 100;
            var year = yearTwoDigit >= 50 ? (currentCentury - 1) * 100 + yearTwoDigit : currentCentury * 100 + yearTwoDigit;

            var tenDigitNumber = $"{yearTwoDigit:D2}{month:D2}{day:D2}{birthNumber}{checksum}";

            return new PersonalNumberParts(year, month, day, birthNumber, checksum, tenDigitNumber);
        }

        // Pattern for YYMMDDBBBC (10 digits without delimiter)
        var noDelimiterPattern = @"^(\d{2})(\d{2})(\d{2})(\d{3})(\d{1})$";
        var noDelimiterMatch = Regex.Match(personalNumber, noDelimiterPattern);

        if (noDelimiterMatch.Success)
        {
            var yearTwoDigit = int.Parse(noDelimiterMatch.Groups[1].Value);
            var month = int.Parse(noDelimiterMatch.Groups[2].Value);
            var day = int.Parse(noDelimiterMatch.Groups[3].Value);
            var birthNumber = noDelimiterMatch.Groups[4].Value;
            var checksum = noDelimiterMatch.Groups[5].Value;

            // Determine full year
            var currentYear = DateTime.Now.Year;
            var currentCentury = currentYear / 100;
            var year = yearTwoDigit >= 50 ? (currentCentury - 1) * 100 + yearTwoDigit : currentCentury * 100 + yearTwoDigit;

            var tenDigitNumber = personalNumber;

            return new PersonalNumberParts(year, month, day, birthNumber, checksum, tenDigitNumber);
        }

        // Pattern for YYYYMMDDBBBC (12 digits without delimiter)
        var fullYearNoDelimiterPattern = @"^(\d{4})(\d{2})(\d{2})(\d{3})(\d{1})$";
        var fullYearNoDelimiterMatch = Regex.Match(personalNumber, fullYearNoDelimiterPattern);

        if (fullYearNoDelimiterMatch.Success)
        {
            var year = int.Parse(fullYearNoDelimiterMatch.Groups[1].Value);
            var month = int.Parse(fullYearNoDelimiterMatch.Groups[2].Value);
            var day = int.Parse(fullYearNoDelimiterMatch.Groups[3].Value);
            var birthNumber = fullYearNoDelimiterMatch.Groups[4].Value;
            var checksum = fullYearNoDelimiterMatch.Groups[5].Value;

            // Create 10-digit version for Luhn validation
            var shortYear = year % 100;
            var tenDigitNumber = $"{shortYear:D2}{month:D2}{day:D2}{birthNumber}{checksum}";

            return new PersonalNumberParts(year, month, day, birthNumber, checksum, tenDigitNumber);
        }

        return null;
    }

    private bool IsValidDate(int year, int month, int day)
    {
        // Basic month validation
        if (month < 1 || month > 12)
            return false;

        // Basic day validation
        if (day < 1 || day > 31)
            return false;

        // More precise date validation
        try
        {
            var date = new DateTime(year, month, day);
            return date <= DateTime.Now.Date; // Can't be born in the future
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidLuhn(string tenDigitNumber)
    {
        if (string.IsNullOrEmpty(tenDigitNumber) || tenDigitNumber.Length != 10)
            return false;

        // Convert to array of integers
        var digits = tenDigitNumber.Select(c => int.Parse(c.ToString())).ToArray();

        // Extract checksum (last digit)
        var providedChecksum = digits[9];

        // Calculate checksum using Luhn algorithm
        // Use the first 9 digits and multiply by 212121212
        var multipliers = new[] { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        var sum = 0;

        for (int i = 0; i < 9; i++)
        {
            var product = digits[i] * multipliers[i];

            // If product is two digits, add the digits together
            if (product >= 10)
            {
                sum += (product / 10) + (product % 10);
            }
            else
            {
                sum += product;
            }
        }

        // Calculate checksum: 10 - (sum % 10), but if result is 10, then checksum is 0
        var calculatedChecksum = (10 - (sum % 10)) % 10;

        return calculatedChecksum == providedChecksum;
    }

    private record PersonalNumberParts(int Year, int Month, int Day, string BirthNumber, string Checksum, string TenDigitNumber);
}
