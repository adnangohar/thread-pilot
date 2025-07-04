using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vehicle.Application.Queries.GetVehicle;

namespace Vehicle.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        services.AddTransient<IValidator<GetVehicleByRegistrationNumberQuery>, GetVehicleByRegistrationNumberQueryValidator>();

        return services;
    }
}
