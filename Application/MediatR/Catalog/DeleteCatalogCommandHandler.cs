using Application.Caching.Interfaces;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class DeleteCatalogCommandHandler<T>(DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<DeleteCatalogCommand<T>, bool>
{
    public async Task<bool> Handle(DeleteCatalogCommand<T> request, CancellationToken cancellationToken)
    {
        var affectedRows = await writer.DeleteAsync(request.Id);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return affectedRows > 0;
    }
}
