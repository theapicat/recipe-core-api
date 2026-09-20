using Application.Results;
using Domain.Recipes;
using MediatR;

namespace Application.MediatR.User.Recipes;

// Erstatter hele oppskriften (også stegene og ingrediensene). Eier kommer fra tokenet.
public record UpdateRecipeCommand(Guid UserId, Guid Id, RecipeRequest Request) : IRequest<Result<Recipe>>;
