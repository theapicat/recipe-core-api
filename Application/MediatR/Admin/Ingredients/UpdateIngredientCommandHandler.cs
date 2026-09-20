using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class UpdateIngredientCommandHandler(DbReader<Ingredient> reader, DbWriter<Ingredient> writer, ICacheService cache)
    : IRequestHandler<UpdateIngredientCommand, Result<Ingredient>>
{
    public async Task<Result<Ingredient>> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var error = IngredientMapper.Validate(request.Ingredient);
        if (error is not null)
            return Result<Ingredient>.Invalid(error);

        if (await reader.GetByIdAsync(request.Id) is null)
            return Result<Ingredient>.NotFound();

        var ingredient = IngredientMapper.ToIngredient(request.Ingredient, request.Id);
        await writer.UpdateAsync(ingredient);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return Result<Ingredient>.Success(ingredient);
    }
}
