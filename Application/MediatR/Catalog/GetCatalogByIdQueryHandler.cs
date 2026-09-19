using MediatR;
using Persistence.Services;

namespace Application.MediatR.Catalog;

public class GetCatalogByIdQueryHandler<T>(DbReader<T> reader)
    : IRequestHandler<GetCatalogByIdQuery<T>, T?>
{
    public Task<T?> Handle(GetCatalogByIdQuery<T> request, CancellationToken cancellationToken)
        => reader.GetByIdAsync(request.Id);
}
