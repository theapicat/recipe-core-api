using Application.MediatR.Catalog;
using Domain.Ingredients;
using Domain.Recipes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DomainUnit = Domain.Units.Unit;
using DomainUnitType = Domain.Units.UnitType;

namespace Application.Extensions;

public static class CatalogExtensions
{
    // Katalogmodeller med full CRUD (lesing med cache + skriving) via den generiske CQRS-malen i
    // Application.MediatR.Catalog, sammen med nøkkeltypen. Legg til nye katalogtyper her etter hvert som de får
    // en tilsvarende Reader/Writer i Persistence.
    // Domain.Units.Unit/UnitType er alias-et siden "Unit" ellers kolliderer med MediatR.Unit.
    private static readonly (Type Entity, Type Key)[] CatalogTypes =
    [
        (typeof(IngredientCategory), typeof(Guid)),
        (typeof(Allergen), typeof(Guid)),
        (typeof(SearchKeyword), typeof(Guid)),
        (typeof(DomainUnitType), typeof(Guid)),
        (typeof(DomainUnit), typeof(Guid)),
        (typeof(RecipeCategory), typeof(Guid)),
        (typeof(NutrientDefinition), typeof(string))
    ];

    // MediatR sin assembly-scanning registrerer ikke ekte åpne generiske handlers der TRequest og
    // TResponse er avledet fra samme delte typeparameter - .NET sin DI-container krever samsvarende
    // arity mellom åpen service-type og åpen implementasjonstype. Derfor lukkes hver handler manuelt
    // her, én gang per katalogtype, i stedet for én håndskrevet handler-klasse per modell.
    public static IServiceCollection AddCatalogHandlers(this IServiceCollection services)
    {
        foreach (var (entity, key) in CatalogTypes)
        {
            AddGetAll(services, entity);
            AddGetById(services, entity, key);
            AddInsert(services, entity, key);
            AddUpdate(services, entity);
            AddDelete(services, entity, key);
        }

        // Ingredienser skrives via egne kommandoer (DTO, transaksjon, cache-invalidering av lista), men
        // lesing gjenbruker malen: lista (lettvekts-projeksjon, cachet) og enkeltoppslag av den fulle modellen.
        AddGetAll(services, typeof(IngredientListItem));
        AddGetById(services, typeof(Ingredient), typeof(Guid));

        return services;
    }

    private static void AddGetAll(IServiceCollection services, Type entity) =>
        services.AddScoped(
            typeof(IRequestHandler<,>).MakeGenericType(
                typeof(GetAllCatalogQuery<>).MakeGenericType(entity),
                typeof(List<>).MakeGenericType(entity)),
            typeof(GetAllCatalogQueryHandler<>).MakeGenericType(entity));

    private static void AddGetById(IServiceCollection services, Type entity, Type key) =>
        services.AddScoped(
            typeof(IRequestHandler<,>).MakeGenericType(
                typeof(GetCatalogByIdQuery<,>).MakeGenericType(entity, key),
                entity),
            typeof(GetCatalogByIdQueryHandler<,>).MakeGenericType(entity, key));

    private static void AddInsert(IServiceCollection services, Type entity, Type key) =>
        services.AddScoped(
            typeof(IRequestHandler<,>).MakeGenericType(
                typeof(InsertCatalogCommand<,>).MakeGenericType(entity, key),
                key),
            typeof(InsertCatalogCommandHandler<,>).MakeGenericType(entity, key));

    private static void AddUpdate(IServiceCollection services, Type entity) =>
        services.AddScoped(
            typeof(IRequestHandler<,>).MakeGenericType(
                typeof(UpdateCatalogCommand<>).MakeGenericType(entity),
                typeof(bool)),
            typeof(UpdateCatalogCommandHandler<>).MakeGenericType(entity));

    private static void AddDelete(IServiceCollection services, Type entity, Type key) =>
        services.AddScoped(
            typeof(IRequestHandler<,>).MakeGenericType(
                typeof(DeleteCatalogCommand<,>).MakeGenericType(entity, key),
                typeof(bool)),
            typeof(DeleteCatalogCommandHandler<,>).MakeGenericType(entity, key));
}
