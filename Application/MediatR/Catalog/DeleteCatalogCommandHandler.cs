using Application.Caching.Interfaces;
using Application.Results;
using Domain;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class DeleteCatalogCommandHandler<T, TKey>(DbReader<T> reader, DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<DeleteCatalogCommand<T, TKey>, Result> where T : IHasUsageMetadata
{
    public async Task<Result> Handle(DeleteCatalogCommand<T, TKey> request, CancellationToken cancellationToken)
    {
        var existing = await reader.GetByIdAsync(request.Id);
        if (existing is null)
            return Result.NotFound();

        if (existing.IsSystem)
            return Result.Conflict("Systemrader (fra seed-data) kan ikke slettes.");
        if (existing.UsageCount > 0)
            return Result.Conflict($"Brukes fortsatt ({existing.UsageCount} referanser) og kan ikke slettes.");

        await writer.DeleteAsync(request.Id);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return Result.Success();
    }
}
