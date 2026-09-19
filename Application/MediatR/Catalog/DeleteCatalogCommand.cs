using MediatR;

namespace Application.MediatR.Catalog;

public record DeleteCatalogCommand<T>(Guid Id) : IRequest<bool>;
