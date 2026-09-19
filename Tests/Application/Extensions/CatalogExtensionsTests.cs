using Application.Extensions;
using Application.MediatR.Catalog;
using Domain.Ingredients;
using Domain.Recipes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DomainUnit = Domain.Units.Unit;
using DomainUnitType = Domain.Units.UnitType;

namespace Tests.Application.Extensions;

public class CatalogExtensionsTests
{
    public static IEnumerable<object[]> CatalogTypes =>
    [
        [typeof(IngredientCategory)],
        [typeof(Allergen)],
        [typeof(SearchKeyword)],
        [typeof(DomainUnitType)],
        [typeof(DomainUnit)],
        [typeof(RecipeCategory)]
    ];

    // Regresjonstest for et reelt problem oppdaget under utvikling: MediatR sin assembly-scanning
    // registrerer ikke åpne generiske handlers der request og response er avledet fra samme
    // typeparameter, siden .NET sin DI-container krever samsvarende arity mellom åpne typer.
    [Theory]
    [MemberData(nameof(CatalogTypes))]
    public void AddCatalogHandlers_RegistersAllFiveHandlers_ForEachCatalogType(Type entityType)
    {
        var services = new ServiceCollection();

        services.AddCatalogHandlers();

        var getAllHandlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(GetAllCatalogQuery<>).MakeGenericType(entityType),
            typeof(List<>).MakeGenericType(entityType));
        var getByIdHandlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(GetCatalogByIdQuery<>).MakeGenericType(entityType), entityType);
        var insertHandlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(InsertCatalogCommand<>).MakeGenericType(entityType), typeof(bool));
        var updateHandlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(UpdateCatalogCommand<>).MakeGenericType(entityType), typeof(bool));
        var deleteHandlerType = typeof(IRequestHandler<,>).MakeGenericType(
            typeof(DeleteCatalogCommand<>).MakeGenericType(entityType), typeof(bool));

        Assert.Contains(services, sd => sd.ServiceType == getAllHandlerType);
        Assert.Contains(services, sd => sd.ServiceType == getByIdHandlerType);
        Assert.Contains(services, sd => sd.ServiceType == insertHandlerType);
        Assert.Contains(services, sd => sd.ServiceType == updateHandlerType);
        Assert.Contains(services, sd => sd.ServiceType == deleteHandlerType);
    }
}
