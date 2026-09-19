using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

namespace Persistence.Extensions;

public static class DatabaseMigrationExtensions
{
    public static void MigrateDatabase(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' er ikke satt opp.");

        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw result.Error;
    }
}
