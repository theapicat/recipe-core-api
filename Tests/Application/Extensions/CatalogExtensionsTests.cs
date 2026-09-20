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
        [typeof(IngredientCategory), typeof(Guid)],
        [typeof(Allergen), typeof(Guid)],
        [typeof(SearchKeyword), typeof(Guid)],
        [typeof(DomainUnitType), typeof(Guid)],
        [typeof(DomainUnit), typeof(Guid)],
        [typeof(RecipeCategory), typeof(Guid)],
        [typeof(NutrientDefinition), typeof(string)]
    ];

    private static Type Handler(Type request, Type response) => typeof(IRequestHandler<,>).MakeGenericType(request, response);

    // Regresjonstest for et reelt problem oppdaget under utvikling: MediatR sin assembly-scanning
    // registrerer ikke åpne generiske handlers der request og response er avledet fra samme
    // typeparameter, siden .NET sin DI-container krever samsvarende arity mellom åpne typer.
    [Theory]
    [MemberData(nameof(CatalogTypes))]
    public void AddCatalogHandlers_RegistersAllFiveHandlers_ForEachCatalogType(Type entity, Type key)
    {
        var services = new ServiceCollection();

        services.AddCatalogHandlers();

        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetAllCatalogQuery<>).MakeGenericType(entity), typeof(List<>).MakeGenericType(entity)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetCatalogByIdQuery<,>).MakeGenericType(entity, key), entity));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(InsertCatalogCommand<,>).MakeGenericType(entity, key), key));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(UpdateCatalogCommand<>).MakeGenericType(entity), typeof(bool)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(DeleteCatalogCommand<,>).MakeGenericType(entity, key), typeof(bool)));
    }

    [Fact]
    public void AddCatalogHandlers_RegistersReadOnlyHandlers_ForIngredients()
    {
        var services = new ServiceCollection();

        services.AddCatalogHandlers();

        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetAllCatalogQuery<IngredientListItem>), typeof(List<IngredientListItem>)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetCatalogByIdQuery<Ingredient, Guid>), typeof(Ingredient)));
        Assert.DoesNotContain(services, sd => sd.ServiceType == Handler(
            typeof(InsertCatalogCommand<Ingredient, Guid>), typeof(Guid)));
    }
}
