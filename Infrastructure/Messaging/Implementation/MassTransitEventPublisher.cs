using Infrastructure.Messaging.Interfaces;
using MassTransit;

namespace Infrastructure.Messaging.Implementation;

public class 
    MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        => publishEndpoint.Publish(@event, cancellationToken);
}
