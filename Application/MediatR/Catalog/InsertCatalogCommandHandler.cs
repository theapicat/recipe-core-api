using Application.Caching.Interfaces;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class InsertCatalogCommandHandler<T>(DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<InsertCatalogCommand<T>, bool>
{
    public async Task<bool> Handle(InsertCatalogCommand<T> request, CancellationToken cancellationToken)
    {
        await writer.AddAsync(request.Entity);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return true;
    }
}
