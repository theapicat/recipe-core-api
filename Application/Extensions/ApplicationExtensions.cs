using Application;
using Application.Caching.Implementation;
using Application.Caching.Interfaces;
using Application.Messaging.Implementation;
using Application.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<ApplicationMarker>();
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        services.AddMassTransitServices(configuration);
        services.AddRealtimeServices();

        return services;
    }
}
