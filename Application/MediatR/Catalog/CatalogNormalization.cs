using Application.Naming;
using Domain;
using DomainUnit = Domain.Units.Unit;

namespace Application.MediatR.Catalog;

// Normaliserer kataloginnhold før skriving: navn lagres alltid med små bokstaver. Enhetens forkortelse er et symbol
// (µg, mg-ATE) og beholder store/små bokstaver - den trimmes bare.
internal static class CatalogNormalization
{
    public static void Apply<T>(T entity)
    {
        if (entity is IHasName named)
            named.Name = NameNormalizer.Normalize(named.Name);

        if (entity is DomainUnit unit)
            unit.Abbreviation = NameNormalizer.Tidy(unit.Abbreviation);
    }
}
