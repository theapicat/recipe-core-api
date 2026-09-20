using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class DeleteIngredientCommandHandler(DbWriter<Ingredient> writer, ICacheService cache)
    : IRequestHandler<DeleteIngredientCommand, Result>
{
    public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var affectedRows = await writer.DeleteAsync(request.Id);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return affectedRows > 0 ? Result.Success() : Result.NotFound();
    }
}
