namespace Domain.Recipes;

// Peker på nøyaktig én av IngredientId eller UnconfirmedIngredientId, aldri begge/ingen.
public class RecipeIngredient
{
    public required Guid Id { get; set; }
    public required Guid RecipeId { get; set; }
    public Guid? IngredientId { get; set; }
    public Guid? UnconfirmedIngredientId { get; set; }

    // 0 = ikke oppgitt / «etter smak» (f.eks. salt). Bidrar ikke til næringsberegningen - næringsverdier er veiledende.
    public required decimal Amount { get; set; }

    public required Guid UnitId { get; set; }
    public string? Note { get; set; }

    // Rekkefølgen brukeren skrev ingrediensene i (1..n), satt av serveren.
    public required int SortOrder { get; set; }

    // Navnet på ingrediensen (offisiell eller brukerens egen) - kun ved lesing, fylt av lesespørringen. Skrives ikke.
    public string? Name { get; set; }
}
