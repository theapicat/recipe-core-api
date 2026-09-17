namespace RecipeCoreApi.Domain.Models.Ingredients;

// En brukeroppgitt ingrediens som ikke finnes i den offisielle katalogen ennå.
// Ikke del av Ingredient-katalogen - kun synlig/brukbar for brukeren som opprettet den, til admin
// enten godkjenner den til en ekte Ingredient eller kobler oppskriften til en eksisterende.
public class UnconfirmedIngredient
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CreatedByUserId { get; set; }

    // Brukeren har bedt om at admin finner offisielle nærings-/allergendata for denne.
    public required bool RequestOfficialData { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
}
