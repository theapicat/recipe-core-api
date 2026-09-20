using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class GetCatalogByIdQueryHandler<T, TKey>(DbReader<T> reader)
    : IRequestHandler<GetCatalogByIdQuery<T, TKey>, T?>
{
    public Task<T?> Handle(GetCatalogByIdQuery<T, TKey> request, CancellationToken cancellationToken)
        => reader.GetByIdAsync(request.Id);
}
