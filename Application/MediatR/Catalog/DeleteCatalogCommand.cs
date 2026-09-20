using MediatR;

namespace Application.MediatR.Catalog;

public record DeleteCatalogCommand<T, TKey>(TKey Id) : IRequest<bool>;
