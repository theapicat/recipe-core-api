using Application.Caching.Interfaces;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class DeleteCatalogCommandHandler<T, TKey>(DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<DeleteCatalogCommand<T, TKey>, bool>
{
    public async Task<bool> Handle(DeleteCatalogCommand<T, TKey> request, CancellationToken cancellationToken)
    {
        var affectedRows = await writer.DeleteAsync(request.Id);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return affectedRows > 0;
    }
}
