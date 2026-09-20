using Domain.Recipes;

namespace Persistence.Interfaces;

// Egen kontrakt (ikke DbReader<T>-malen): oppskrifter er brukereide, så alle spørringer tar eier og filtrerer på den. En annen
// brukers oppskrift gir null/tom liste - identisk med en id som ikke finnes.
public interface IRecipeReader
{
    // Lettvekts-lista (uten steg og ingredienser), sortert på tittel. Hele lista hentes; klienten filtrerer og søker.
    Task<List<RecipeListItem>> GetListByOwnerAsync(Guid ownerUserId);

    // Hele oppskriften med steg og ingredienslinjer (med ingrediensnavn), hentet i ett kall.
    Task<Recipe?> GetByIdAsync(Guid id, Guid ownerUserId);

    Task<int> CountByOwnerAsync(Guid ownerUserId);

    // Rådata til næringsberegningen (linjer, porsjoner, næringsverdier), eller null hvis oppskriften ikke finnes / ikke er eierens.
    Task<RecipeNutritionInput?> GetNutritionInputAsync(Guid id, Guid ownerUserId);
}
