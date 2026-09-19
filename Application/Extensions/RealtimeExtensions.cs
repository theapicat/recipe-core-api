using Application.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class RealtimeExtensions
{
    public static IServiceCollection AddRealtimeServices(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }

    public static IEndpointRouteBuilder MapRealtimeHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<RecipeHub>("/hubs/recipe");

        return endpoints;
    }
}
