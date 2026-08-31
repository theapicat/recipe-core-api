using MediatR;

namespace Application.MediatR.Public.ContactForm;

public record SendContactFormCommand(
    string Name,
    string Email,
    string Subject,
    string Message,
    DateTime SubmittedAt
    ) : IRequest<bool>;