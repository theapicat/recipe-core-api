using MediatR;

namespace Application.MediatR.Catalog;

public record InsertCatalogCommand<T>(T Entity) : IRequest<bool>;
