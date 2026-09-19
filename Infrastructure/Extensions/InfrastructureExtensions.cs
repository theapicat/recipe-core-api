using Infrastructure.Caching.Implementation;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Messaging.Implementation;
using Infrastructure.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        services.AddMassTransitServices(configuration);
        services.AddRealtimeServices();

        return services;
    }
}
