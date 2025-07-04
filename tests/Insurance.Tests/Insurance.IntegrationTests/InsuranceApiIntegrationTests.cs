using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Vehicle.Contracts;
using FluentValidation;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Insurance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DomainInsuranceType = Insurance.Domain.Enums.InsuranceType;
using Insurance.Application.Common;
using System.Text.Json;

namespace Insurance.IntegrationTests;

public class InsuranceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IVehicleService> _vehicleServiceMock;
    private readonly Mock<IValidator<GetPersonInsurancesRequest>> _validatorMock;
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _vehicleServiceMock = new Mock<IVehicleService>();
        _validatorMock = new Mock<IValidator<GetPersonInsurancesRequest>>();
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetPersonInsurancesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Configure JSON options to match the API configuration
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        _factory = factory;
        _testFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all Entity Framework related services
                var descriptorsToRemove = services.Where(d => 
                    d.ServiceType == typeof(DbContextOptions<InsuranceDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(InsuranceDbContext) ||
                    d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                    d.ImplementationType == typeof(InsuranceDbContext)).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                var dbcontext = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<InsuranceDbContext>));
                if (dbcontext != null) services.Remove(dbcontext);
                
                // Remove other services
                var vehicleServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleService));
                if (vehicleServiceDescriptor != null) services.Remove(vehicleServiceDescriptor);

                var validatorDescriptor = services.SingleOrDefault(
                   d => d.ServiceType == typeof(IValidator<GetPersonInsurancesRequest>));
                if (validatorDescriptor != null) services.Remove(validatorDescriptor);

                // Add test services
                services.AddDbContext<InsuranceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                services.AddSingleton(_vehicleServiceMock.Object);
                services.AddSingleton(_validatorMock.Object);
            });
        });
        
        _client = _testFactory.CreateClient();
    }

    private void EnsureTestDataExists()
    {
        using var scope = _testFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
        
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        SeedTestData(context);
    }

    [Fact]
    public async Task GetInsurances_ExistingPerson_ReturnsOkWithPolicies()
    {
        // Arrange
        EnsureTestDataExists();
        
        var personalId = "19870604-5088";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };
        
        _vehicleServiceMock
            .Setup(x => x.GetVehicleInfoAsync("CAR123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleResponse
            {
                RegistrationNumber = "CAR123",
                Make = "Toyota",
                Model = "Camry",
                Year = 2023,
                Color = "Blue"
            });

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResult>(_jsonOptions);
        content.Should().NotBeNull();
        content!.PersonalIdentificationNumber.Should().Be(personalId);
        content.Insurances.Should().HaveCount(3);
        content.TotalMonthlyCost.Should().Be(60m);
        
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == Insurance.Contracts.InsuranceType.Car);
        carPolicy.Should().NotBeNull();
        carPolicy!.VehicleInfo.Should().NotBeNull();
        carPolicy.VehicleInfo!.Make.Should().Be("Toyota");
    }

    [Fact]
    public async Task GetInsurances_NonExistentPerson_ReturnsOkWithEmptyResult()
    {
        // Arrange
        EnsureTestDataExists();
        
        var personalId = "19990829-0274";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResult>(_jsonOptions);
        content.Should().NotBeNull();
        content!.PersonalIdentificationNumber.Should().Be(personalId);
        content.Insurances.Should().BeEmpty();
        content.TotalMonthlyCost.Should().Be(0m);
    }

    [Fact]
    public async Task GetInsurances_InvalidPersonalId_ReturnsBadRequest()
    {
        // Arrange
        EnsureTestDataExists();
        
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = "ABC" }; // Too short

          _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetPersonInsurancesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [
                    new FluentValidation.Results.ValidationFailure("PersonalIdentificationNumber", "Invalid personal identification number format.")
                ]));

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInsurances_VehicleServiceUnavailable_StillReturnsInsurances()
    {
        // Arrange
        EnsureTestDataExists();
        
        var personalId = "19990822-4984";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };
        
        _vehicleServiceMock
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResult>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Insurances.Should().HaveCount(1);
        
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == Insurance.Contracts.InsuranceType.Car);
        carPolicy.Should().NotBeNull();
        carPolicy!.VehicleInfo.Should().BeNull(); // Vehicle info not available
    }

    private void SeedTestData(InsuranceDbContext context)
    {
        var person1 = new PersonalIdentificationNumber("19870604-5088");
        var person2 = new PersonalIdentificationNumber("19990822-4984");
        
        var insurances = new Insurance.Domain.Entities.Insurance[]
        {
            new Insurance.Domain.Entities.Insurance(person1, 20m, DomainInsuranceType.PersonalHealth),
            new Insurance.Domain.Entities.Insurance(person1, 15m, DomainInsuranceType.Pet),
            new Insurance.Domain.Entities.Insurance(person1, 25m, DomainInsuranceType.Car, "CAR123"),
            new Insurance.Domain.Entities.Insurance(person2, 30m, DomainInsuranceType.Car, "CAR456")
        };
        
        context.Insurances.AddRange(insurances);
        context.SaveChanges();
    }
}