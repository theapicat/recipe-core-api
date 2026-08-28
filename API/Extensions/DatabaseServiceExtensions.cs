using System.Data;
using Npgsql;

namespace API.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' finnes ikke i appsettings.");
            
            return new NpgsqlConnection(connectionString);
        });

        return services;
    }
}