using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

// Kun mens ingen forespørsel er sendt (status NotRequested).
public record RenameOwnUnconfirmedIngredientCommand(Guid UserId, Guid Id, string Name)
    : IRequest<Result<UnconfirmedIngredient>>;
