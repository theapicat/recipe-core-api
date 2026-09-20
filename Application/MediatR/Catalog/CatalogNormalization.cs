using Application.Naming;
using Domain;
using DomainUnit = Domain.Units.Unit;

namespace Application.MediatR.Catalog;

// Normaliserer kataloginnhold før skriving: navn (og enhetens forkortelse) lagres alltid med små bokstaver.
internal static class CatalogNormalization
{
    public static void Apply<T>(T entity)
    {
        if (entity is IHasName named)
            named.Name = NameNormalizer.Normalize(named.Name);

        if (entity is DomainUnit unit)
            unit.Abbreviation = NameNormalizer.Normalize(unit.Abbreviation);
    }
}
