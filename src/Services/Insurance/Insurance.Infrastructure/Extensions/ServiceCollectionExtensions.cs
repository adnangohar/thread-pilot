using System.Text.Json;
using Insurance.Core.Interfaces;
using Insurance.Core.Repositories;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories;
using Insurance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insurance.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<InsuranceDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IInsuranceRepository, InsuranceRepository>();

        // External Services
        services.AddHttpClient<IVehicleService, VehicleServiceClient>(client =>
        {
            var baseUrl = configuration["VehicleService:BaseUrl"] ?? throw new InvalidOperationException("VehicleService:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddStandardResilienceHandler();

        // Configure JsonSerializerOptions for VehicleServiceClient
        services.AddSingleton(new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        return services;
    }
}
