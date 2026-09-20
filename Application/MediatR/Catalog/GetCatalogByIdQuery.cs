using MediatR;

namespace Application.MediatR.Catalog;

// Går alltid direkte mot databasen, ikke cache - brukes for admin-redigering og skal alltid være ferskt.
// Bevisst uten "Cache" i navnet siden den nettopp ikke bruker cachen.
public record GetCatalogByIdQuery<T, TKey>(TKey Id) : IRequest<T?>;
