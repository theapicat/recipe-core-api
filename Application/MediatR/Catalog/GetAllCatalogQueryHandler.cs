using Application.Caching.Interfaces;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class GetAllCatalogQueryHandler<T>(DbReader<T> reader, ICacheService cache)
    : IRequestHandler<GetAllCatalogQuery<T>, List<T>>
{
    public async Task<List<T>> Handle(GetAllCatalogQuery<T> request, CancellationToken cancellationToken)
    {
        var cacheKey = CatalogCacheKey.ForAll<T>();

        var cached = cache.Get<List<T>>(cacheKey);
        if (cached is not null)
            return cached;

        var items = await reader.GetAllAsync();
        cache.Set(cacheKey, items);
        return items;
    }
}
