using System.Net;
using System.Net.Http.Json;
using System.Linq;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Vehicle.Contracts;
using FluentValidation;

namespace Insurance.IntegrationTests;

public class InsuranceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IVehicleService> _vehicleServiceMock;
    private readonly Mock<IValidator<GetPersonInsurancesRequest>> _validatorMock;

    public InsuranceApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _vehicleServiceMock = new Mock<IVehicleService>();
        _validatorMock = new Mock<IValidator<GetPersonInsurancesRequest>>();
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetPersonInsurancesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing services
                // var dbDescriptor = services.SingleOrDefault(
                //     d => d.ServiceType == typeof(DbContextOptions<InsuranceDbContext>));
                // if (dbDescriptor != null) services.Remove(dbDescriptor);

                var vehicleServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleService));
                if (vehicleServiceDescriptor != null) services.Remove(vehicleServiceDescriptor);

                var validatorMock = services.SingleOrDefault(
                   d => d.ServiceType == typeof(IValidator<GetPersonInsurancesRequest>));
                if (vehicleServiceDescriptor != null) services.Remove(vehicleServiceDescriptor);

                // Add test services
                // services.AddDbContext<InsuranceDbContext>(options =>
                // {
                //     options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                // });

                services.AddSingleton(_vehicleServiceMock.Object);
                services.AddSingleton(_validatorMock.Object);

                // Seed data
                // var sp = services.BuildServiceProvider();
                // using var scope = sp.CreateScope();
                // var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
                // context.Database.EnsureCreated();
                // SeedTestData(context);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetInsurances_ExistingPerson_ReturnsOkWithPolicies()
    {
        // Arrange
        var personalId = "19900101-1234";
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
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResponse>();
        content.Should().NotBeNull();
        content!.PersonalIdentificationNumber.Should().Be(personalId);
        content.Insurances.Should().HaveCount(3);
        content.TotalMonthlyCost.Should().Be(60m);
        
        var carPolicy = content.Insurances.OfType<CarInsuranceResponse>().FirstOrDefault();
        carPolicy.Should().NotBeNull();
        carPolicy!.Vehicle.Should().NotBeNull();
        carPolicy.Vehicle!.Make.Should().Be("Toyota");
    }

    [Fact]
    public async Task GetInsurances_NonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personalId = "19800101-9999";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInsurances_InvalidPersonalId_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = "ABC" }; // Too short

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInsurances_VehicleServiceUnavailable_StillReturnsInsurances()
    {
        // Arrange
        var personalId = "19900101-1234";
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = personalId };
        
        _vehicleServiceMock
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var response = await _client.PostAsJsonAsync("/insurances", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<PersonInsurancesResponse>();
        content.Should().NotBeNull();
        content!.Insurances.Should().HaveCount(3);
        
        var carPolicy = content.Insurances.OfType<CarInsuranceResponse>().FirstOrDefault();
        carPolicy.Should().NotBeNull();
        carPolicy!.Vehicle.Should().BeNull(); // Vehicle info not available
    }

    // private void SeedTestData(InsuranceDbContext context)
    // {
    //     var policies = new[]
    //     {
    //         new InsurancePolicy("19900101-1234", InsuranceType.Health),
    //         new InsurancePolicy("19900101-1234", InsuranceType.Pet),
    //         new InsurancePolicy("19900101-1234", InsuranceType.Car, "CAR123"),
    //         new InsurancePolicy("19800101-9999", InsuranceType.Health)
    //     };

    //     context.InsurancePolicies.AddRange(policies);
    //     context.SaveChanges();
    // }
}