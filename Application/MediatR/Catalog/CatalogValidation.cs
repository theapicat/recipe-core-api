using Application.Naming;
using Domain;
using Domain.Units;
using MediatR;
using DomainUnit = Domain.Units.Unit;

namespace Application.MediatR.Catalog;

// Delt validering for katalogskriving, kjørt av InsertCatalogCommandHandler/UpdateCatalogCommandHandler før normalisering.
// Navnesjekken gjelder alle katalogtyper (flyttet hit fra kontrolleren, som ikke skal inneholde forretningsregler). Enhet
// har i tillegg egne regler som krever et oppslag mot enhetstype-katalogen.
public static class CatalogValidation
{
    public static async Task<string?> ValidateAsync<T>(IMediator mediator, T entity, CancellationToken cancellationToken)
    {
        if (entity is IHasName { Name: var name } && string.IsNullOrWhiteSpace(name))
            return "Navn må oppgis.";

        return entity is DomainUnit unit ? await ValidateUnitAsync(mediator, unit, cancellationToken) : null;
    }

    private static async Task<string?> ValidateUnitAsync(IMediator mediator, DomainUnit unit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(unit.Abbreviation))
            return "Forkortelse må oppgis.";
        if (unit.BaseUnitRatio <= 0)
            return "Forholdstallet må være større enn 0.";

        var unitTypes = await mediator.Send(new GetAllCatalogQuery<UnitType>(), cancellationToken);
        var unitType = unitTypes.FirstOrDefault(t => t.Id == unit.UnitTypeId);
        if (unitType is null)
            return "Enhetstypen finnes ikke.";

        // "Antall" regnes ikke om (stk, skive, boks ...) - et forholdstall ulikt 1 ville gitt feil næringsberegning.
        if (unitType.Name == UnitTypeNames.Count && unit.BaseUnitRatio != 1)
            return "Enheter av typen antall regnes ikke om - forholdstallet må være 1.";

        return null;
    }
}
