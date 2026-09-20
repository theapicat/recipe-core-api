using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class CreateIngredientCommandHandler(DbWriter<Ingredient> writer, ICacheService cache)
    : IRequestHandler<CreateIngredientCommand, Result<Ingredient>>
{
    public async Task<Result<Ingredient>> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var error = IngredientMapper.Validate(request.Ingredient);
        if (error is not null)
            return Result<Ingredient>.Invalid(error);

        var ingredient = IngredientMapper.ToIngredient(request.Ingredient, Guid.CreateVersion7());
        await writer.AddAsync(ingredient);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return Result<Ingredient>.Success(ingredient);
    }
}
