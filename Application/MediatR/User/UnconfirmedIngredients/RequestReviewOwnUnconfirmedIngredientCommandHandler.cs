using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class RequestReviewOwnUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer)
    : IRequestHandler<RequestReviewOwnUnconfirmedIngredientCommand, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        RequestReviewOwnUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await reader.GetByIdAsync(request.Id);
        if (ingredient is null || ingredient.CreatedByUserId != request.UserId)
            return Result<UnconfirmedIngredient>.NotFound();

        if (ingredient.ReviewStatus != UnconfirmedIngredientStatus.NotRequested)
            return Result<UnconfirmedIngredient>.Conflict("Det er allerede sendt en forespørsel for denne ingrediensen.");

        if (await reader.CountByUserAsync(request.UserId, UnconfirmedIngredientStatus.Pending)
            >= UnconfirmedIngredientLimits.MaxPendingPerUser)
            return Result<UnconfirmedIngredient>.Conflict(
                $"Du kan ha maks {UnconfirmedIngredientLimits.MaxPendingPerUser} forespørsler til behandling om gangen.");

        if (!await writer.RequestReviewAsync(request.Id, request.UserId))
            return Result<UnconfirmedIngredient>.Conflict("Forespørselen kunne ikke sendes.");

        ingredient.ReviewStatus = UnconfirmedIngredientStatus.Pending;
        return Result<UnconfirmedIngredient>.Success(ingredient);
    }
}
