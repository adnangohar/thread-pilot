using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Vehicle.Infrastructure.Persistence;
using Vehicle.Domain.ValueObjects;
using Vehicle.Application.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Vehicle.IntegrationTests;

public class VehicleApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _testFactory;

    public VehicleApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _testFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all Entity Framework related services
                var descriptorsToRemove = services.Where(d => 
                    d.ServiceType == typeof(DbContextOptions<VehicleDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(VehicleDbContext) ||
                    d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                    d.ImplementationType == typeof(VehicleDbContext)).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                var dbcontext = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<VehicleDbContext>));
                if (dbcontext != null) services.Remove(dbcontext);

                // Add test services
                services.AddDbContext<VehicleDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        
        _client = _testFactory.CreateClient();
    }

    private void EnsureTestDataExists()
    {
        using var scope = _testFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
        
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        SeedTestData(context);
    }

    [Fact]
    public async Task GetVehicle_ExistingRegistrationNumber_ReturnsOkWithVehicle()
    {
        // Arrange
        EnsureTestDataExists();
        
        var registrationNumber = "ABD123";

        // Act
        var response = await _client.GetAsync($"/vehicles/{registrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<VehicleResult>();
        content.Should().NotBeNull();
        content!.RegistrationNumber.Should().Be(registrationNumber);
        content.Make.Should().Be("Toyota");
        content.Model.Should().Be("Camry");
        content.Year.Should().Be(2023);
        content.Color.Should().Be("Blue");
    }

    [Fact]
    public async Task GetVehicle_NonExistentRegistrationNumber_ReturnsNotFound()
    {
        // Arrange
        EnsureTestDataExists();
        
        var registrationNumber = "XYZ999";

        // Act
        var response = await _client.GetAsync($"/vehicles/{registrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVehicle_InvalidRegistrationNumber_ReturnsNotFound()
    {
        // Arrange
        EnsureTestDataExists();
        
        var invalidRegistrationNumber = "A"; // Too short

        // Act
        var response = await _client.GetAsync($"/vehicles/{invalidRegistrationNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void SeedTestData(VehicleDbContext context)
    {
        var vehicles = new Vehicle.Domain.Entities.Vehicle[]
        {
            new Vehicle.Domain.Entities.Vehicle(
                new RegistrationNumber("ABD123"), 
                "Toyota", 
                "Camry", 
                2023, 
                "Blue"),
            new Vehicle.Domain.Entities.Vehicle(
                new RegistrationNumber("DEF456"), 
                "Honda", 
                "Civic", 
                2022, 
                "Red"),
            new Vehicle.Domain.Entities.Vehicle(
                new RegistrationNumber("GHI789"), 
                "Ford", 
                "Focus", 
                2021, 
                "White")
        };
        
        context.Vehicles.AddRange(vehicles);
        context.SaveChanges();
    }
}
