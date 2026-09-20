namespace Domain.Ingredients;

// En brukeroppgitt ingrediens som ikke finnes i den offisielle katalogen ennå.
// Ikke del av Ingredient-katalogen - kun synlig/brukbar for brukeren som opprettet den, til admin
// enten godkjenner den til en ekte Ingredient, kobler den til en eksisterende (merge) eller avslår den.
// Raden beholdes etter avgjørelse som historikk, slik at brukeren kan se utfallet.
public class UnconfirmedIngredient : IHasId<Guid>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public required UnconfirmedIngredientStatus ReviewStatus { get; set; }

    // Kun satt når ReviewStatus er Rejected.
    public string? RejectionReason { get; set; }

    // Satt når admin har avgjort forespørselen.
    public DateTimeOffset? ReviewedAt { get; set; }

    // Kun satt når ReviewStatus er Approved eller Merged - den offisielle ingrediensen dette endte som.
    public Guid? ResolvedIngredientId { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
}
