using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class GetOwnUnconfirmedIngredientQueryHandler(IUnconfirmedIngredientReader reader)
    : IRequestHandler<GetOwnUnconfirmedIngredientQuery, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(GetOwnUnconfirmedIngredientQuery request, CancellationToken cancellationToken)
    {
        var ingredient = await reader.GetByIdAsync(request.Id);

        return ingredient is null || ingredient.CreatedByUserId != request.UserId
            ? Result<UnconfirmedIngredient>.NotFound()
            : Result<UnconfirmedIngredient>.Success(ingredient);
    }
}
