using Application.Results;
using Domain.Recipes;
using MediatR;

namespace Application.MediatR.User.Recipes;

// Eier kommer fra tokenet (UserId), aldri fra request-body.
public record CreateRecipeCommand(Guid UserId, RecipeRequest Request) : IRequest<Result<Recipe>>;
