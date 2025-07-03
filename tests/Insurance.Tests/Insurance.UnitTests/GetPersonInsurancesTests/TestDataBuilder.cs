using Insurance.Domain.Entities;
using Insurance.Domain.ValueObjects;
using Insurance.Contracts;
using Vehicle.Contracts;
using Bogus;

namespace Insurance.UnitTests.GetPersonInsurancesTests;

public static class TestDataBuilder
{
    private static readonly Faker Faker = new Faker();

    public static PersonalIdentificationNumber CreateValidPin(string? pin = null)
    {
        var pinValue = pin ?? Faker.Random.String2(9, "0123456789");
        return new PersonalIdentificationNumber(pinValue);
    }

    public static CarInsurance CreateCarInsurance(string? pin = null, string? registrationNumber = null)
    {
        var personalPin = CreateValidPin(pin);
        var regNumber = registrationNumber ?? Faker.Vehicle.Vin().Substring(0, 7);
        return new CarInsurance(personalPin, regNumber);
    }

    public static PetInsurance CreatePetInsurance(string? pin = null, string? petName = null, string? petType = null)
    {
        var personalPin = CreateValidPin(pin);
        var name = petName ?? Faker.Name.FirstName();
        var type = petType ?? Faker.PickRandom("Dog", "Cat", "Bird", "Fish", "Rabbit");
        return new PetInsurance(personalPin, name, type);
    }

    public static PersonalHealthInsurance CreateHealthInsurance(string? pin = null, string? coverageLevel = null)
    {
        var personalPin = CreateValidPin(pin);
        var coverage = coverageLevel ?? Faker.PickRandom("Basic", "Standard", "Premium", "Platinum");
        return new PersonalHealthInsurance(personalPin, coverage);
    }

    public static CarInsuranceResponse CreateCarInsuranceResponse(decimal? monthlyCost = null, VehicleResponse? vehicle = null)
    {
        return new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = monthlyCost ?? 30m, // Keep original default for test predictability
            Vehicle = vehicle
        };
    }

    public static PetInsuranceResponse CreatePetInsuranceResponse(decimal? monthlyCost = null, string? petName = null, string? petType = null)
    {
        return new PetInsuranceResponse
        {
            Type = "Pet",
            MonthlyCost = monthlyCost ?? 10m, // Keep original default for test predictability
            PetName = petName ?? Faker.Name.FirstName(),
            PetType = petType ?? Faker.PickRandom("Dog", "Cat", "Bird", "Fish", "Rabbit")
        };
    }

    public static PersonalHealthInsuranceResponse CreateHealthInsuranceResponse(decimal? monthlyCost = null, string? coverageLevel = null)
    {
        return new PersonalHealthInsuranceResponse
        {
            Type = "Health",
            MonthlyCost = monthlyCost ?? 20m, // Keep original default for test predictability
            CoverageLevel = coverageLevel ?? Faker.PickRandom("Basic", "Standard", "Premium", "Platinum")
        };
    }

    public static VehicleResponse CreateVehicleResponse(string? registrationNumber = null, string? make = null, string? model = null)
    {
        return new VehicleResponse
        {
            RegistrationNumber = registrationNumber ?? Faker.Vehicle.Vin().Substring(0, 7),
            Make = make ?? Faker.Vehicle.Manufacturer(),
            Model = model ?? Faker.Vehicle.Model(),
            Year = Faker.Random.Int(2000, DateTime.Now.Year),
            Color = Faker.Commerce.Color()
        };
    }

    /// <summary>
    /// Creates multiple car insurances for testing
    /// </summary>
    public static List<CarInsurance> CreateCarInsurances(int count, string? pin = null)
    {
        var personalPin = CreateValidPin(pin);
        return Enumerable.Range(1, count)
            .Select(_ => new CarInsurance(personalPin, Faker.Vehicle.Vin().Substring(0, 7)))
            .ToList();
    }

    /// <summary>
    /// Creates multiple pet insurances for testing
    /// </summary>
    public static List<PetInsurance> CreatePetInsurances(int count, string? pin = null)
    {
        var personalPin = CreateValidPin(pin);
        return Enumerable.Range(1, count)
            .Select(_ => new PetInsurance(personalPin, Faker.Name.FirstName(), Faker.PickRandom("Dog", "Cat", "Bird", "Fish", "Rabbit")))
            .ToList();
    }

    /// <summary>
    /// Creates multiple health insurances for testing
    /// </summary>
    public static List<PersonalHealthInsurance> CreateHealthInsurances(int count, string? pin = null)
    {
        var personalPin = CreateValidPin(pin);
        return Enumerable.Range(1, count)
            .Select(_ => new PersonalHealthInsurance(personalPin, Faker.PickRandom("Basic", "Standard", "Premium", "Platinum")))
            .ToList();
    }

    /// <summary>
    /// Creates multiple car insurance responses for testing
    /// </summary>
    public static List<CarInsuranceResponse> CreateCarInsuranceResponses(int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateCarInsuranceResponse())
            .ToList();
    }

    /// <summary>
    /// Creates multiple pet insurance responses for testing
    /// </summary>
    public static List<PetInsuranceResponse> CreatePetInsuranceResponses(int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreatePetInsuranceResponse())
            .ToList();
    }

    /// <summary>
    /// Creates multiple health insurance responses for testing
    /// </summary>
    public static List<PersonalHealthInsuranceResponse> CreateHealthInsuranceResponses(int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateHealthInsuranceResponse())
            .ToList();
    }

    /// <summary>
    /// Creates multiple vehicle responses for testing
    /// </summary>
    public static List<VehicleResponse> CreateVehicleResponses(int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateVehicleResponse())
            .ToList();
    }

    // Random data generation methods for cases where truly random values are needed
    
    /// <summary>
    /// Creates a car insurance response with random monthly cost
    /// </summary>
    public static CarInsuranceResponse CreateRandomCarInsuranceResponse(VehicleResponse? vehicle = null)
    {
        return new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = Faker.Random.Decimal(10, 100),
            Vehicle = vehicle
        };
    }

    /// <summary>
    /// Creates a pet insurance response with random monthly cost
    /// </summary>
    public static PetInsuranceResponse CreateRandomPetInsuranceResponse(string? petName = null, string? petType = null)
    {
        return new PetInsuranceResponse
        {
            Type = "Pet",
            MonthlyCost = Faker.Random.Decimal(5, 50),
            PetName = petName ?? Faker.Name.FirstName(),
            PetType = petType ?? Faker.PickRandom("Dog", "Cat", "Bird", "Fish", "Rabbit")
        };
    }

    /// <summary>
    /// Creates a health insurance response with random monthly cost
    /// </summary>
    public static PersonalHealthInsuranceResponse CreateRandomHealthInsuranceResponse(string? coverageLevel = null)
    {
        return new PersonalHealthInsuranceResponse
        {
            Type = "Health",
            MonthlyCost = Faker.Random.Decimal(15, 200),
            CoverageLevel = coverageLevel ?? Faker.PickRandom("Basic", "Standard", "Premium", "Platinum")
        };
    }
}
