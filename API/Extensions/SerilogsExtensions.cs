using Serilog;

namespace API.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host)
    {
        host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        return host;
    }
}