using Application.Caching.Interfaces;
using Application.MediatR.Admin.Ingredients;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.Admin.UnconfirmedIngredients;

public class ApproveUnconfirmedIngredientCommandHandler(
    IUnconfirmedIngredientReader reader,
    IUnconfirmedIngredientWriter writer,
    ICacheService cache,
    IMediator mediator,
    TimeProvider timeProvider)
    : IRequestHandler<ApproveUnconfirmedIngredientCommand, Result<Ingredient>>
{
    public async Task<Result<Ingredient>> Handle(ApproveUnconfirmedIngredientCommand request, CancellationToken cancellationToken)
    {
        var error = IngredientMapper.Validate(request.Ingredient);
        if (error is not null)
            return Result<Ingredient>.Invalid(error);

        var stub = await reader.GetByIdAsync(request.Id);
        if (stub is null)
            return Result<Ingredient>.NotFound();

        if (stub.ReviewStatus != UnconfirmedIngredientStatus.Pending)
            return Result<Ingredient>.Conflict("Bare ventende forespørsler kan godkjennes.");

        var fkError = await IngredientForeignKeyValidator.ValidateAsync(mediator, request.Ingredient, null, cancellationToken);
        if (fkError is not null)
            return Result<Ingredient>.Invalid(fkError);

        // Godkjent fra en brukers ubekreftede ingrediens - ikke offisiell (det er forbeholdt Matvaretabellen-seeden).
        var ingredient = IngredientMapper.ToIngredient(request.Ingredient, Guid.CreateVersion7(), isOfficial: false, timeProvider.GetUtcNow());

        var resolved = await writer.ResolveAsync(
            stub.Id, UnconfirmedIngredientStatus.Approved, ingredient.Id, ingredient, timeProvider.GetUtcNow());
        if (!resolved)
            return Result<Ingredient>.Conflict("Forespørselen er ikke lenger ventende.");

        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());
        return Result<Ingredient>.Success(ingredient);
    }
}
