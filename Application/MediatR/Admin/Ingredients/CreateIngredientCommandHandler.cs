using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class CreateIngredientCommandHandler(DbWriter<Ingredient> writer, ICacheService cache, IMediator mediator, TimeProvider timeProvider)
    : IRequestHandler<CreateIngredientCommand, Result<Ingredient>>
{
    public async Task<Result<Ingredient>> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var error = IngredientMapper.Validate(request.Ingredient);
        if (error is not null)
            return Result<Ingredient>.Invalid(error);

        var fkError = await IngredientForeignKeyValidator.ValidateAsync(mediator, request.Ingredient, null, cancellationToken);
        if (fkError is not null)
            return Result<Ingredient>.Invalid(fkError);

        // isOfficial er alltid false her - kun seed-data er offisiell (se seed_10..25 og Documentation/06).
        var ingredient = IngredientMapper.ToIngredient(request.Ingredient, Guid.CreateVersion7(), isOfficial: false, timeProvider.GetUtcNow());
        await writer.AddAsync(ingredient);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return Result<Ingredient>.Success(ingredient);
    }
}
