using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vehicle.Core.Queries.GetVehicle;

namespace Vehicle.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IGetVehicleByRegistrationNumberQueryHandler, GetVehicleByRegistrationNumberQueryHandler>();
        services.AddTransient<IValidator<GetVehicleByRegistrationNumberQuery>, GetVehicleByRegistrationNumberQueryValidator>();

        return services;
    }
}
