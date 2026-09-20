using Domain.Recipes;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class GetOwnRecipesQueryHandler(IRecipeReader reader) : IRequestHandler<GetOwnRecipesQuery, List<RecipeListItem>>
{
    public Task<List<RecipeListItem>> Handle(GetOwnRecipesQuery request, CancellationToken cancellationToken) =>
        reader.GetListByOwnerAsync(request.UserId);
}
