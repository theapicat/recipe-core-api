namespace Infrastructure.Messaging.Interfaces;

/// <summary>
/// Publiserer hendelser til meldingsbussen. Skjuler MassTransit/RabbitMQ-detaljer for handlers.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
