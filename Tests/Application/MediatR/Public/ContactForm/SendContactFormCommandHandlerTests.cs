using Application.MediatR.Public.ContactForm;
using Application.Messaging.Interfaces;
using Contracts.Event;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Public.ContactForm;

public class SendContactFormCommandHandlerTests
{
    [Fact]
    public async Task Handle_PublishesContactFormSubmittedEvent_AndReturnsTrue()
    {
        var logger = Substitute.For<ILogger<SendContactFormCommandHandler>>();
        var publisher = Substitute.For<IEventPublisher>();
        var handler = new SendContactFormCommandHandler(logger, publisher);
        var command = new SendContactFormCommand("Ola", "ola@example.com", "Hei", "Melding", DateTime.UtcNow);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        await publisher.Received(1).PublishAsync(
            Arg.Is<ContactFormSubmittedEvent>(e => e.Name == command.Name && e.Email == command.Email),
            Arg.Any<CancellationToken>());
    }
}
