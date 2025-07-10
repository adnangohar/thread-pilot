using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Moq;
using FluentValidation;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DomainInsuranceType = Insurance.Core.Enums.InsuranceType;
using System.Text.Json;
using Insurance.Core.Interfaces;
using Insurance.Api.Contracts;
using Insurance.Core.Common;
using Insurance.Core.ValueObjects;

namespace Insurance.IntegrationTests;

public class InsuranceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IVehicleService> _vehicleServiceMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly Mock<IValidator<GetPersonInsurancesRequest>> _validatorMock;
    private readonly WebApplicationFactory<Program> _testFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    public const string EnableDetailedVehicleInfo = "EnableDetailedVehicleInfo";

    public InsuranceApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _vehicleServiceMock = new Mock<IVehicleService>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _validatorMock = new Mock<IValidator<GetPersonInsurancesRequest>>();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetPersonInsurancesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Set default feature flag state - can be overridden in individual tests
        _featureManagerMock.Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

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

                // Remove other services to replace with mocks
                var vehicleServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleService));
                if (vehicleServiceDescriptor != null) services.Remove(vehicleServiceDescriptor);

                var featureManagerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFeatureManager));
                if (featureManagerDescriptor != null) services.Remove(featureManagerDescriptor);

                var validatorDescriptor = services.SingleOrDefault(
                   d => d.ServiceType == typeof(IValidator<GetPersonInsurancesRequest>));
                if (validatorDescriptor != null) services.Remove(validatorDescriptor);

                // Add test services
                services.AddDbContext<InsuranceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                services.AddSingleton(_vehicleServiceMock.Object);
                services.AddSingleton(_featureManagerMock.Object);
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
        
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == DomainInsuranceType.Car);
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
        
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == DomainInsuranceType.Car);
        carPolicy.Should().NotBeNull();
        carPolicy!.VehicleInfo.Should().BeNull(); // Vehicle info not available
    }

    [Fact]
    public async Task GetInsurances_WhenFeatureFlagDisabled_ShouldNotIncludeVehicleInfo()
    {
        // Arrange
        EnsureTestDataExists();
        
        // Override feature flag for this test only
        _featureManagerMock.Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var personalId = "19870604-5088";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResult>(_jsonOptions);
        content.Should().NotBeNull();
        content!.PersonalIdentificationNumber.Should().Be(personalId);
        content.Insurances.Should().HaveCount(3);
        
        // Verify that car insurance does NOT have vehicle info when feature is disabled
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == DomainInsuranceType.Car);
        carPolicy.Should().NotBeNull();
        carPolicy!.VehicleInfo.Should().BeNull();
        
        // Verify that vehicle service was NOT called
        _vehicleServiceMock.Verify(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Reset feature flag mock for other tests
        ResetFeatureFlagToDefault();
    }

    [Fact]
    public async Task GetInsurances_WhenFeatureFlagEnabled_ShouldIncludeVehicleInfo()
    {
        // Arrange
        EnsureTestDataExists();
        
        // Explicitly set feature flag to sd for this test
        _featureManagerMock.Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var personalId = "19900906-3356";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };
        
        _vehicleServiceMock
            .Setup(x => x.GetVehicleInfoAsync("CAR789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleResponse
            {
                RegistrationNumber = "CAR789",
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
        content.Insurances.Should().HaveCount(1);
        
        // Verify that car insurance HAS vehicle info when feature is enabled
        var carPolicy = content.Insurances.FirstOrDefault(x => x.Type == DomainInsuranceType.Car);
        carPolicy.Should().NotBeNull();
        carPolicy!.VehicleInfo.Should().NotBeNull();
        carPolicy.VehicleInfo!.Make.Should().Be("Toyota");
        carPolicy.VehicleInfo.Model.Should().Be("Camry");
        
        // Verify that vehicle service WAS called
        _vehicleServiceMock.Verify(x => x.GetVehicleInfoAsync("CAR789", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Helper method to reset feature flag mock to default state between tests
    /// </summary>
    private void ResetFeatureFlagToDefault()
    {
        _featureManagerMock.Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SeedTestData(InsuranceDbContext context)
    {
        var person1 = new PersonalIdentificationNumber("19870604-5088");
        var person2 = new PersonalIdentificationNumber("19990822-4984");
        var person3 = new PersonalIdentificationNumber("19900906-3356");
        
        var insurances = new Insurance.Core.Entities.Insurance[]
        {
            new Insurance.Core.Entities.Insurance(person1, 20m, DomainInsuranceType.PersonalHealth),
            new Insurance.Core.Entities.Insurance(person1, 15m, DomainInsuranceType.Pet),
            new Insurance.Core.Entities.Insurance(person1, 25m, DomainInsuranceType.Car, "CAR123"),
            new Insurance.Core.Entities.Insurance(person2, 30m, DomainInsuranceType.Car, "CAR456"),
            new Insurance.Core.Entities.Insurance(person3, 30m, DomainInsuranceType.Car, "CAR789")
        };
        
        context.Insurances.AddRange(insurances);
        context.SaveChanges();
    }
}