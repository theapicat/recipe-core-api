using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public class RejectUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer,
    TimeProvider timeProvider)
    : IRequestHandler<RejectUnconfirmedIngredientCommand, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        RejectUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var stub = await reader.GetByIdAsync(request.Id);
        if (stub is null)
            return Result<UnconfirmedIngredient>.NotFound();

        if (stub.ReviewStatus != UnconfirmedIngredientStatus.Pending)
            return Result<UnconfirmedIngredient>.Conflict("Bare ventende forespørsler kan avslås.");

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        var reviewedAt = timeProvider.GetUtcNow();
        if (!await writer.RejectAsync(stub.Id, reason, reviewedAt))
            return Result<UnconfirmedIngredient>.Conflict("Forespørselen er ikke lenger ventende.");

        stub.ReviewStatus = UnconfirmedIngredientStatus.Rejected;
        stub.RejectionReason = reason;
        stub.ReviewedAt = reviewedAt;
        return Result<UnconfirmedIngredient>.Success(stub);
    }
}
