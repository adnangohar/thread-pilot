using Bogus;

namespace Insurance.UnitTests;

public static class TestDataBuilder
{
    private static readonly Faker Faker = new Faker();
    public static string GenerateSwedishPin()
    {
        // Generates a Swedish personal number in the format "YYYYMMDD-XXXX"
        var date = Faker.Date.PastOffset(40, DateTime.Now.AddYears(-18)).DateTime;
        var datePart = date.ToString("yyMMdd");
        var serial = Faker.Random.Number(1000, 9999).ToString("D4");
        return $"{datePart}-{serial}";
    }

    public static string GenerateCarRegNumber()
    {
        // Generates either "ABC 123" or "ABC 12A"
        var letters = Faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (Faker.Random.Bool())
        {
            var numbers = Faker.Random.Number(100, 999).ToString("D3");
            return $"{letters} {numbers}";
        }
        else
        {
            var numbers = Faker.Random.Number(10, 99).ToString("D2");
            var letter = Faker.Random.ArrayElement("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
            return $"{letters} {numbers}{letter}";
        }
    }
}