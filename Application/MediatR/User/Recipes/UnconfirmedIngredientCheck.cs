using Domain.Ingredients;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

// En oppskrift kan bare bruke brukerens EGNE ubekreftede ingredienser, og ikke en som allerede er løst (Approved/Merged - da er den
// erstattet av en offisiell ingrediens). En annen brukers ingrediens gir samme melding som en som ikke finnes, så eksistens ikke avsløres.
internal static class UnconfirmedIngredientCheck
{
    public static async Task<string?> ValidateAsync(IUnconfirmedIngredientReader reader, Guid userId, RecipeRequest request)
    {
        var usedIds = request.Ingredients
            .Where(line => line.UnconfirmedIngredientId.HasValue)
            .Select(line => line.UnconfirmedIngredientId!.Value)
            .Distinct()
            .ToList();
        if (usedIds.Count == 0)
            return null;

        var own = (await reader.GetByUserAsync(userId)).ToDictionary(u => u.Id);
        foreach (var id in usedIds)
        {
            if (!own.TryGetValue(id, out var stub))
                return "En av ingrediensene du har valgt finnes ikke.";

            if (stub.ReviewStatus is UnconfirmedIngredientStatus.Approved or UnconfirmedIngredientStatus.Merged)
                return $"Ingrediensen «{stub.Name}» er godkjent og erstattet av en offisiell ingrediens - bruk den i stedet.";
        }

        return null;
    }
}
