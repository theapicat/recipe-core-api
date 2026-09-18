using Contracts.Event;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.MediatR.Public.ContactForm;

public class SendContactFormCommandHandler(
    ILogger<SendContactFormCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<SendContactFormCommand, bool>
{
    public async Task<bool> Handle(SendContactFormCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Behandler innkommende kontaktskjema for e-post: {Email}", request.Email);

        await UseMassTransit(request);

        return true;
    }

    private async Task UseMassTransit(SendContactFormCommand request)
    {
        var message = new ContactFormSubmittedEvent
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            SubmittedAt = request.SubmittedAt
        };
        
        await publishEndpoint.Publish(message);
        logger.LogInformation("ContactFormSubmittedEvent ble publisert til RabbitMQ for {Email}", request.Email);
    }
}