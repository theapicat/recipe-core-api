using Application.Results;
using MediatR;

namespace Application.MediatR.Catalog;

public record UpdateCatalogCommand<T>(T Entity) : IRequest<Result>;
