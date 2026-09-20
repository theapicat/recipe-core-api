using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.UnconfirmedIngredients;

public class GetOwnUnconfirmedIngredientsQueryHandler(IUnconfirmedIngredientReader reader)
    : IRequestHandler<GetOwnUnconfirmedIngredientsQuery, List<UnconfirmedIngredient>>
{
    public Task<List<UnconfirmedIngredient>> Handle(GetOwnUnconfirmedIngredientsQuery request, CancellationToken cancellationToken)
        => reader.GetByUserAsync(request.UserId);
}
