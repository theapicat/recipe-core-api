using Application.Results;
using Domain.Recipes;
using MediatR;

namespace Application.MediatR.User.Recipes;

// Næring regnes ut på forespørsel (lagres ikke i oppskriften). En annen brukers oppskrift gir NotFound - identisk med en id som ikke finnes.
public record GetRecipeNutritionQuery(Guid UserId, Guid Id) : IRequest<Result<RecipeNutrition>>;
