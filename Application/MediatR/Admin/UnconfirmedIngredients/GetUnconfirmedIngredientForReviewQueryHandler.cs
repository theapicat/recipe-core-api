using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public class GetUnconfirmedIngredientForReviewQueryHandler(IUnconfirmedIngredientReader reader)
    : IRequestHandler<GetUnconfirmedIngredientForReviewQuery, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        GetUnconfirmedIngredientForReviewQuery request, CancellationToken cancellationToken)
    {
        var ingredient = await reader.GetByIdAsync(request.Id);
        return ingredient is null ? Result<UnconfirmedIngredient>.NotFound() : Result<UnconfirmedIngredient>.Success(ingredient);
    }
}
