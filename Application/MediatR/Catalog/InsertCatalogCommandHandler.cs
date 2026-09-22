using Application.Caching.Interfaces;
using Application.Results;
using Domain;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class InsertCatalogCommandHandler<T, TKey>(DbWriter<T> writer, ICacheService cache, IMediator mediator)
    : IRequestHandler<InsertCatalogCommand<T, TKey>, Result<TKey>> where T : IHasId<TKey>
{
    public async Task<Result<TKey>> Handle(InsertCatalogCommand<T, TKey> request, CancellationToken cancellationToken)
    {
        var error = await CatalogValidation.ValidateAsync(mediator, request.Entity, cancellationToken);
        if (error is not null)
            return Result<TKey>.Invalid(error);

        if (request.Entity is IHasId<Guid> guidEntity)
            guidEntity.Id = Guid.CreateVersion7();

        // Serverstyrt, uansett hva klienten sendte - se IHasUsageMetadata. Uten denne nullstillingen ville en
        // klient-oppgitt isSystem/usageCount blitt ekko-et tilbake i 201-svaret (CreatedAtAction under sender
        // request.Entity), selv om selve raden alltid får is_system = false fra kolonnens DEFAULT.
        if (request.Entity is IHasUsageMetadata usageEntity)
        {
            usageEntity.IsSystem = false;
            usageEntity.UsageCount = 0;
        }

        CatalogNormalization.Apply(request.Entity);
        await writer.AddAsync(request.Entity);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return Result<TKey>.Success(request.Entity.Id);
    }
}
