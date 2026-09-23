using Application.Messaging.Consumers.AdminActions;
using Application.Messaging.Consumers.SystemActions;
using Application.Messaging.Consumers.UserActions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Kontosletting i recipe-auth-api - fire varianter (bruker/system/admin/admin+svarteliste), alle rutet til
            // samme DeleteAllUserDataCommand. Dette er tjenestens første innkommende forbruker (ellers bare publisering).
            //
            // Eksplisitt Endpoint-navn er PÅKREVD her: MassTransits standardnavngiving bruker kun forbrukerens
            // klassenavn (uten navnerom), og recipe-notification-service har en forbruker med nøyaktig samme
            // klassenavn for hver av disse fire hendelsene. Uten eget navn ville denne tjenesten og
            // notification-service havnet i samme kø og KONKURRERT om meldingene (hver hendelse går da til bare én av
            // de to tjenestene, tilfeldig) i stedet for at begge får sin egen kø bundet til samme utveksling (fan-out,
            // slik det skal være). Bekreftet levende 2026-09-23: en midlertidig instans av denne tjenesten uten dette
            // navnet viste seg som forbruker #2 på notification-service sin ekte "AccountDeletedByUser"-kø i det
            // delte RabbitMQ-oppsettet.
            x.AddConsumer<AccountDeletedByUserConsumer>().Endpoint(e => e.Name = "CoreApi-AccountDeletedByUser");
            x.AddConsumer<AccountDeletedBySystemConsumer>().Endpoint(e => e.Name = "CoreApi-AccountDeletedBySystem");
            x.AddConsumer<UserAccountDeletedByAdminConsumer>().Endpoint(e => e.Name = "CoreApi-UserAccountDeletedByAdmin");
            x.AddConsumer<UserDeletedAndBlacklistedByAdminConsumer>().Endpoint(e => e.Name = "CoreApi-UserDeletedAndBlacklistedByAdmin");

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var port = ushort.Parse(configuration["RabbitMQ:Port"] ?? "5672");
                var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
                var username = configuration["RabbitMQ:Username"] ?? "rabbit_user";
                var password = configuration["RabbitMQ:Password"] ?? "rabbit_secure_password_dev";

                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
