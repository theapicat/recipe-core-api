using Application.Results;
using Domain.Recipes;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class GetRecipeNutritionQueryHandler(IRecipeReader reader)
    : IRequestHandler<GetRecipeNutritionQuery, Result<RecipeNutrition>>
{
    public async Task<Result<RecipeNutrition>> Handle(GetRecipeNutritionQuery request, CancellationToken cancellationToken)
    {
        var input = await reader.GetNutritionInputAsync(request.Id, request.UserId);
        return input is null
            ? Result<RecipeNutrition>.NotFound()
            : Result<RecipeNutrition>.Success(RecipeNutritionCalculator.Calculate(input));
    }
}
