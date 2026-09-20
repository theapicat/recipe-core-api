using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

// NotRequested -> Pending. Send en forespørsel om at admin tar ingrediensen inn i den offisielle katalogen.
public record RequestReviewOwnUnconfirmedIngredientCommand(Guid UserId, Guid Id)
    : IRequest<Result<UnconfirmedIngredient>>;
