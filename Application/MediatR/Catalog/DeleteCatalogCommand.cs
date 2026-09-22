using Application.Results;
using Domain;
using MediatR;

namespace Application.MediatR.Catalog;

public record DeleteCatalogCommand<T, TKey>(TKey Id) : IRequest<Result> where T : IHasUsageMetadata;
