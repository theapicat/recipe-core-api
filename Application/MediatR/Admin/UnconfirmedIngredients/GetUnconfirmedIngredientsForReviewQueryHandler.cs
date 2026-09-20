using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public class GetUnconfirmedIngredientsForReviewQueryHandler(IUnconfirmedIngredientReader reader)
    : IRequestHandler<GetUnconfirmedIngredientsForReviewQuery, List<UnconfirmedIngredient>>
{
    public Task<List<UnconfirmedIngredient>> Handle(
        GetUnconfirmedIngredientsForReviewQuery request, CancellationToken cancellationToken)
        => reader.GetByStatusAsync(request.Status);
}
