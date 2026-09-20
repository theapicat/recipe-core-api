using Application.Results;
using Domain.Recipes;
using MediatR;

namespace Application.MediatR.User.Recipes;

// En annen brukers oppskrift gir NotFound - identisk med en id som ikke finnes, så eksistens ikke avsløres.
public record GetOwnRecipeQuery(Guid UserId, Guid Id) : IRequest<Result<Recipe>>;
