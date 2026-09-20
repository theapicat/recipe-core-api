using Domain.Recipes;

namespace Persistence.Interfaces;

public interface IRecipeWriter
{
    // Oppskriften og alle barna (steg, ingredienslinjer) skrives i én transaksjon.
    Task AddAsync(Recipe recipe);

    // Oppdaterer raden (kun hvis den tilhører recipe.OwnerUserId) og erstatter alle barna, i én transaksjon. false = finnes
    // ikke eller er ikke din (ingenting er endret).
    Task<bool> UpdateAsync(Recipe recipe);

    // false = finnes ikke eller er ikke din.
    Task<bool> SetFavoriteAsync(Guid id, Guid ownerUserId, bool isFavorite);

    // Steg og ingredienslinjer følger med. false = finnes ikke eller er ikke din.
    Task<bool> DeleteAsync(Guid id, Guid ownerUserId);
}
