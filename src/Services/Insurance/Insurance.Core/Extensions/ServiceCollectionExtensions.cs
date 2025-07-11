using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Insurance.Core.Queries.GetPersonInsurances;

namespace Insurance.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IGetPersonInsurancesQueryHandler, GetPersonInsurancesQueryHandler>();
        services.AddTransient<IValidator<GetPersonInsurancesQuery>, GetPersonInsurancesQueryValidator>();
        
        return services;
    }
}
