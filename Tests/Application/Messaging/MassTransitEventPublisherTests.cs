using Application.Messaging.Implementation;
using MassTransit;
using NSubstitute;
using Xunit;

namespace Tests.Application.Messaging;

public class MassTransitEventPublisherTests
{
    private record TestEvent(string Value);

    [Fact]
    public async Task PublishAsync_DelegatesToPublishEndpoint()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var publisher = new MassTransitEventPublisher(publishEndpoint);
        var message = new TestEvent("hello");
        using var cts = new CancellationTokenSource();

        await publisher.PublishAsync(message, cts.Token);

        await publishEndpoint.Received(1).Publish(message, cts.Token);
    }
}
