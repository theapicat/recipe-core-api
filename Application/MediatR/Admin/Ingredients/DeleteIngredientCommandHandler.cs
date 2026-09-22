using Application.Caching.Interfaces;
using Application.MediatR.Catalog;
using Application.Results;
using Domain.Ingredients;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Admin.Ingredients;

public class DeleteIngredientCommandHandler(DbReader<Ingredient> reader, DbWriter<Ingredient> writer, ICacheService cache)
    : IRequestHandler<DeleteIngredientCommand, Result>
{
    public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var existing = await reader.GetByIdAsync(request.Id);
        if (existing is null)
            return Result.NotFound();

        if (existing.UsageCount > 0)
            return Result.Conflict($"Brukes fortsatt ({existing.UsageCount} referanser) og kan ikke slettes.");

        await writer.DeleteAsync(request.Id);
        cache.Remove(CatalogCacheKey.ForAll<IngredientListItem>());

        return Result.Success();
    }
}
