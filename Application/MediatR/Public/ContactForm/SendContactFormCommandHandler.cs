using Contracts.Event;
using Infrastructure.Messaging.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.MediatR.Public.ContactForm;

public class SendContactFormCommandHandler(
    ILogger<SendContactFormCommandHandler> logger,
    IEventPublisher eventPublisher)
    : IRequestHandler<SendContactFormCommand, bool>
{
    public async Task<bool> Handle(SendContactFormCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Behandler innkommende kontaktskjema for e-post: {Email}", request.Email);

        await UseMassTransit(request, cancellationToken);

        return true;
    }

    private async Task UseMassTransit(SendContactFormCommand request, CancellationToken cancellationToken)
    {
        var message = new ContactFormSubmittedEvent
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            SubmittedAt = request.SubmittedAt
        };

        await eventPublisher.PublishAsync(message, cancellationToken);
        logger.LogInformation("ContactFormSubmittedEvent ble publisert til RabbitMQ for {Email}", request.Email);
    }
}