using Application.MediatR.Catalog;
using Application.MediatR.Ingredients;
using Domain.Ingredients;
using MediatR;
using NSubstitute;
using Xunit;

namespace Tests.Application.MediatR.Ingredients;

public class SearchIngredientsQueryHandlerTests
{
    private static readonly Guid Gluten = Guid.NewGuid();
    private static readonly Guid Lactose = Guid.NewGuid();
    private static readonly Guid Vegetables = Guid.NewGuid();
    private static readonly Guid Dairy = Guid.NewGuid();
    private static readonly SearchKeyword Root = new() { Id = Guid.NewGuid(), Name = "rotfrukt" };

    private static readonly IngredientListItem Carrot = Item("Gulrot", Vegetables, [], [Root.Id]);
    private static readonly IngredientListItem Milk = Item("Melk", Dairy, [Lactose], []);
    private static readonly IngredientListItem Bread = Item("Brød", Vegetables, [Gluten], []);

    private static IngredientListItem Item(string name, Guid category, Guid[] allergens, Guid[] keywords) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        CategoryId = category,
        PrimaryUnitTypeId = Guid.NewGuid(),
        DefaultUnitId = Guid.NewGuid(),
        EnergyKcal = 1,
        IsVerified = true,
        IsOfficial = false,
        AllergenIds = allergens,
        SearchKeywordIds = keywords
    };

    private static (SearchIngredientsQueryHandler Handler, IMediator Mediator) Create()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllCatalogQuery<IngredientListItem>>(), Arg.Any<CancellationToken>())
            .Returns([Carrot, Milk, Bread]);
        mediator.Send(Arg.Any<GetAllCatalogQuery<SearchKeyword>>(), Arg.Any<CancellationToken>())
            .Returns([Root]);
        return (new SearchIngredientsQueryHandler(mediator), mediator);
    }

    private static Task<List<IngredientListItem>> Search(SearchIngredientsQuery query) =>
        Create().Handler.Handle(query, CancellationToken.None);

    [Fact]
    public async Task NoFilters_ReturnsEverything_WithoutLoadingKeywords()
    {
        var (handler, mediator) = Create();

        var result = await handler.Handle(new SearchIngredientsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        await mediator.DidNotReceive().Send(Arg.Any<GetAllCatalogQuery<SearchKeyword>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Name_MatchesPartiallyAndIgnoringCase()
    {
        var result = await Search(new SearchIngredientsQuery(Name: "GUL"));

        Assert.Equal([Carrot], result);
    }

    [Fact]
    public async Task Name_AlsoMatchesSearchKeywordNames()
    {
        var result = await Search(new SearchIngredientsQuery(Name: "rotfr"));

        Assert.Equal([Carrot], result);
    }

    [Fact]
    public async Task CategoryId_FiltersOnCategory()
    {
        var result = await Search(new SearchIngredientsQuery(CategoryId: Vegetables));

        Assert.Equal([Carrot, Bread], result);
    }

    [Fact]
    public async Task AllergenId_KeepsOnlyIngredientsContainingTheAllergen()
    {
        var result = await Search(new SearchIngredientsQuery(AllergenId: Lactose));

        Assert.Equal([Milk], result);
    }

    [Fact]
    public async Task ExcludeAllergenIds_DropsIngredientsContainingAnyOfThem()
    {
        var result = await Search(new SearchIngredientsQuery(ExcludeAllergenIds: [Gluten, Lactose]));

        Assert.Equal([Carrot], result);
    }

    [Fact]
    public async Task SearchKeywordId_FiltersOnKeyword()
    {
        var result = await Search(new SearchIngredientsQuery(SearchKeywordId: Root.Id));

        Assert.Equal([Carrot], result);
    }

    [Fact]
    public async Task Filters_AreCombinedWithAnd()
    {
        var result = await Search(new SearchIngredientsQuery(CategoryId: Vegetables, ExcludeAllergenIds: [Gluten]));

        Assert.Equal([Carrot], result);
    }
}
