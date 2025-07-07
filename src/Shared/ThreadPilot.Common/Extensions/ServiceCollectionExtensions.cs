using Microsoft.Extensions.DependencyInjection;
using ThreadPilot.Common.Abstractions;

namespace ThreadPilot.Common.Extensions;

 public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            return services;
        }
}
