using Application.Caching.Interfaces;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class UpdateCatalogCommandHandler<T>(DbWriter<T> writer, ICacheService cache)
    : IRequestHandler<UpdateCatalogCommand<T>, bool>
{
    public async Task<bool> Handle(UpdateCatalogCommand<T> request, CancellationToken cancellationToken)
    {
        CatalogNormalization.Apply(request.Entity);
        await writer.UpdateAsync(request.Entity);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return true;
    }
}
