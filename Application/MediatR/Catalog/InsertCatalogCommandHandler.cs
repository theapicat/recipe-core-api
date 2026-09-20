using Application.Caching.Interfaces;
using Domain;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class InsertCatalogCommandHandler<T, TKey>(DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<InsertCatalogCommand<T, TKey>, TKey> where T : IHasId<TKey>
{
    public async Task<TKey> Handle(InsertCatalogCommand<T, TKey> request, CancellationToken cancellationToken)
    {
        if (request.Entity is IHasId<Guid> guidEntity)
            guidEntity.Id = Guid.CreateVersion7();

        await writer.AddAsync(request.Entity);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return request.Entity.Id;
    }
}
