using Application.Extensions;
using Application.MediatR.Catalog;
using Application.Results;
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
        [typeof(RecipeCategory), typeof(Guid)]
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
            typeof(InsertCatalogCommand<,>).MakeGenericType(entity, key), typeof(Result<>).MakeGenericType(key)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(UpdateCatalogCommand<>).MakeGenericType(entity), typeof(Result)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(DeleteCatalogCommand<,>).MakeGenericType(entity, key), typeof(Result)));
    }

    [Fact]
    public void AddCatalogHandlers_RegistersOnlyReadHandlers_ForNutrientDefinitions()
    {
        var services = new ServiceCollection();

        services.AddCatalogHandlers();

        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetAllCatalogQuery<NutrientDefinition>), typeof(List<NutrientDefinition>)));
        Assert.Contains(services, sd => sd.ServiceType == Handler(
            typeof(GetCatalogByIdQuery<NutrientDefinition, string>), typeof(NutrientDefinition)));
        Assert.DoesNotContain(services, sd => sd.ServiceType == Handler(
            typeof(InsertCatalogCommand<NutrientDefinition, string>), typeof(Result<string>)));
        Assert.DoesNotContain(services, sd => sd.ServiceType == Handler(
            typeof(UpdateCatalogCommand<NutrientDefinition>), typeof(Result)));
        // Ingen tilsvarende sjekk for DeleteCatalogCommand<NutrientDefinition, string>: den generiske typen krever nå
        // IHasUsageMetadata, som NutrientDefinition ikke implementerer - umulig å konstruere i det hele tatt, så
        // AddCatalogHandlers kan strukturelt aldri registrere en slik handler.
    }

    // Næringsgrupper er nøstet i næringsstoffet og har ingen egne handlers/endepunkter.
    [Fact]
    public void AddCatalogHandlers_RegistersNothing_ForNutrientGroups()
    {
        var services = new ServiceCollection();

        services.AddCatalogHandlers();

        Assert.DoesNotContain(services, sd => sd.ServiceType.IsGenericType && sd.ServiceType.GenericTypeArguments
            .Any(t => t.IsGenericType && t.GenericTypeArguments.Contains(typeof(NutrientGroup))));
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
            typeof(InsertCatalogCommand<Ingredient, Guid>), typeof(Result<Guid>)));
    }
}
