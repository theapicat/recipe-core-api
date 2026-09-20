using Domain.Ingredients;
using MediatR;

namespace Application.MediatR.Ingredients;

// Alle filtre er valgfrie og kombineres med OG. Uten filtre returneres hele lista.
// Name matcher både ingrediensnavn og søkeord (SearchKeyword) - delvis treff, uavhengig av store/små bokstaver.
// AllergenId = kun ingredienser som inneholder allergenet. ExcludeAllergenIds = utelat ingredienser som inneholder
// noen av dem (kostholds-/sikkerhetsfilter).
public record SearchIngredientsQuery(
    string? Name = null,
    Guid? CategoryId = null,
    Guid? AllergenId = null,
    IReadOnlyCollection<Guid>? ExcludeAllergenIds = null,
    Guid? SearchKeywordId = null) : IRequest<List<IngredientListItem>>;
