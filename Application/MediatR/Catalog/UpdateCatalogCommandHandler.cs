using Application.Caching.Interfaces;
using Application.Results;
using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class UpdateCatalogCommandHandler<T>(DbWriter<T> writer, ICacheService cache, IMediator mediator)
    : IRequestHandler<UpdateCatalogCommand<T>, Result>
{
    public async Task<Result> Handle(UpdateCatalogCommand<T> request, CancellationToken cancellationToken)
    {
        var error = await CatalogValidation.ValidateAsync(mediator, request.Entity, cancellationToken);
        if (error is not null)
            return Result.Invalid(error);

        CatalogNormalization.Apply(request.Entity);
        await writer.UpdateAsync(request.Entity);
        cache.Remove(CatalogCacheKey.ForAll<T>());
        return Result.Success();
    }
}
