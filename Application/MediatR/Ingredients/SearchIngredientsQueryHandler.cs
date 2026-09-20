using Application.MediatR.Catalog;
using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Ingredients;

// Henter den cachede lettvekts-lista og filtrerer i minnet (alle katalogene er små nok til å ligge i minnet).
// Går via mediator slik at cache-aside-logikken i GetAllCatalogQueryHandler gjenbrukes, ikke dupliseres.
public class SearchIngredientsQueryHandler(IMediator mediator)
    : IRequestHandler<SearchIngredientsQuery, List<IngredientListItem>>
{
    public async Task<List<IngredientListItem>> Handle(SearchIngredientsQuery request, CancellationToken cancellationToken)
    {
        var items = await mediator.Send(new GetAllCatalogQuery<IngredientListItem>(), cancellationToken);
        IEnumerable<IngredientListItem> result = items;

        var name = request.Name?.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            var keywords = await mediator.Send(new GetAllCatalogQuery<SearchKeyword>(), cancellationToken);
            var matchingKeywordIds = keywords
                .Where(k => k.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Id)
                .ToHashSet();

            result = result.Where(i =>
                i.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                i.SearchKeywordIds.Any(matchingKeywordIds.Contains));
        }

        if (request.CategoryId is { } categoryId)
            result = result.Where(i => i.CategoryId == categoryId);

        if (request.AllergenId is { } allergenId)
            result = result.Where(i => i.AllergenIds.Contains(allergenId));

        if (request.ExcludeAllergenIds is { Count: > 0 } excluded)
            result = result.Where(i => !i.AllergenIds.Any(excluded.Contains));

        if (request.SearchKeywordId is { } keywordId)
            result = result.Where(i => i.SearchKeywordIds.Contains(keywordId));

        return result.ToList();
    }
}
