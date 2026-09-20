using Application.Results;
using Domain.Recipes;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class GetOwnRecipeQueryHandler(IRecipeReader reader) : IRequestHandler<GetOwnRecipeQuery, Result<Recipe>>
{
    public async Task<Result<Recipe>> Handle(GetOwnRecipeQuery request, CancellationToken cancellationToken)
    {
        var recipe = await reader.GetByIdAsync(request.Id, request.UserId);
        return recipe is null ? Result<Recipe>.NotFound() : Result<Recipe>.Success(recipe);
    }
}
