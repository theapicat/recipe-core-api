using Application.Results;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.User.UnconfirmedIngredients;

// En annen brukers ingrediens gir NotFound - identisk med en id som ikke finnes, så eksistens ikke avsløres.
public record GetOwnUnconfirmedIngredientQuery(Guid UserId, Guid Id) : IRequest<Result<UnconfirmedIngredient>>;
