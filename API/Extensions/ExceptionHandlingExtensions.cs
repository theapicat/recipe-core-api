using API.ExceptionHandlers;

namespace API.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<PostgresExceptionHandler>();

        return services;
    }
}
