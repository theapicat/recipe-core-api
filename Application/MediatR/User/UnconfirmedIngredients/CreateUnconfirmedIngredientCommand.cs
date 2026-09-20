using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

// UserId kommer alltid fra tokenet (satt av kontrolleren), aldri fra request-body.
public record CreateUnconfirmedIngredientCommand(Guid UserId, string Name, bool RequestReview)
    : IRequest<Result<UnconfirmedIngredient>>;
