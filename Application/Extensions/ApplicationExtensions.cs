using Application;
using Application.Caching.Interfaces;
using Application.Caching.Services;
using Application.Messaging.Implementation;
using Application.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

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
        services.AddPersistenceServices();
        services.AddCatalogHandlers();

        return services;
    }
}
