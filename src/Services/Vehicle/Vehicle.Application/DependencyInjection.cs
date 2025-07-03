using Microsoft.Extensions.DependencyInjection;

namespace Vehicle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application specific dependencies
        
        return services;
    }
}
