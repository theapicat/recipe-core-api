using Domain.Recipes;
using MediatR;

namespace Application.MediatR.User.Recipes;

// Hele lista (lettvekts-elementer) til den innloggede brukeren - klienten filtrerer og søker selv. Begrenset av RecipeLimits.MaxPerUser.
public record GetOwnRecipesQuery(Guid UserId) : IRequest<List<RecipeListItem>>;
