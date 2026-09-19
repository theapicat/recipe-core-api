using MediatR;

namespace Application.MediatR.Catalog;

public record GetAllCatalogQuery<T> : IRequest<List<T>>;
