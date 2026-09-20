using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

public record GetOwnUnconfirmedIngredientsQuery(Guid UserId) : IRequest<List<UnconfirmedIngredient>>;
