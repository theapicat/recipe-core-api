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
    // Alle katalogmodeller som bruker den generiske CQRS-malen i Application.MediatR.Catalog.
    // Legg til nye katalogtyper her etter hvert som de får en tilsvarende Reader/Writer i Persistence.
    // Domain.Units.Unit/UnitType er alias-et siden "Unit" ellers kolliderer med MediatR.Unit.
    private static readonly Type[] CatalogTypes =
    [
        typeof(IngredientCategory),
        typeof(Allergen),
        typeof(SearchKeyword),
        typeof(DomainUnitType),
        typeof(DomainUnit),
        typeof(RecipeCategory)
    ];

    // MediatR sin assembly-scanning registrerer ikke ekte åpne generiske handlers der TRequest og
    // TResponse er avledet fra samme delte typeparameter - .NET sin DI-container krever samsvarende
    // arity mellom åpen service-type og åpen implementasjonstype. Derfor lukkes hver handler manuelt
    // her, én gang per katalogtype, i stedet for én håndskrevet handler-klasse per modell.
    public static IServiceCollection AddCatalogHandlers(this IServiceCollection services)
    {
        foreach (var entityType in CatalogTypes)
        {
            services.AddScoped(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(GetAllCatalogQuery<>).MakeGenericType(entityType),
                    typeof(List<>).MakeGenericType(entityType)),
                typeof(GetAllCatalogQueryHandler<>).MakeGenericType(entityType));

            services.AddScoped(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(GetCatalogByIdQuery<>).MakeGenericType(entityType),
                    entityType),
                typeof(GetCatalogByIdQueryHandler<>).MakeGenericType(entityType));

            services.AddScoped(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(InsertCatalogCommand<>).MakeGenericType(entityType),
                    typeof(bool)),
                typeof(InsertCatalogCommandHandler<>).MakeGenericType(entityType));

            services.AddScoped(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(UpdateCatalogCommand<>).MakeGenericType(entityType),
                    typeof(bool)),
                typeof(UpdateCatalogCommandHandler<>).MakeGenericType(entityType));

            services.AddScoped(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(DeleteCatalogCommand<>).MakeGenericType(entityType),
                    typeof(bool)),
                typeof(DeleteCatalogCommandHandler<>).MakeGenericType(entityType));
        }

        return services;
    }
}
