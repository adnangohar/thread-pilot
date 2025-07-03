using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vehicle.Domain.Repositories;
using Vehicle.Infrastructure.Repositories;

namespace Vehicle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IVehicleRepository, InMemoryVehicleRepository>();
        return services;
    }
}
