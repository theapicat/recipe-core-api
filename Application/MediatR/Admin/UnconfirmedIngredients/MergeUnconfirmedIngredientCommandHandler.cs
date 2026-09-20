using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;
using Persistence.Services;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public class MergeUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer,
    DbReader<Ingredient> ingredientReader,
    TimeProvider timeProvider)
    : IRequestHandler<MergeUnconfirmedIngredientCommand, Result<UnconfirmedIngredient>>
{
    public async Task<Result<UnconfirmedIngredient>> Handle(
        MergeUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var stub = await reader.GetByIdAsync(request.Id);
        if (stub is null)
            return Result<UnconfirmedIngredient>.NotFound();

        if (stub.ReviewStatus != UnconfirmedIngredientStatus.Pending)
            return Result<UnconfirmedIngredient>.Conflict("Bare ventende forespørsler kan slås sammen.");

        if (await ingredientReader.GetByIdAsync(request.IngredientId) is null)
            return Result<UnconfirmedIngredient>.Invalid("Ingrediensen som skal kobles til finnes ikke.");

        var reviewedAt = timeProvider.GetUtcNow();
        if (!await writer.ResolveAsync(stub.Id, UnconfirmedIngredientStatus.Merged, request.IngredientId, null, reviewedAt))
            return Result<UnconfirmedIngredient>.Conflict("Forespørselen er ikke lenger ventende.");

        stub.ReviewStatus = UnconfirmedIngredientStatus.Merged;
        stub.ResolvedIngredientId = request.IngredientId;
        stub.ReviewedAt = reviewedAt;
        return Result<UnconfirmedIngredient>.Success(stub);
    }
}
