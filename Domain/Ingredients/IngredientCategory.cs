namespace Domain.Ingredients;

public class IngredientCategory : IHasId<Guid>, IHasName, IHasUsageMetadata
{
    // Tildeles av serveren ved opprettelse (Guid.CreateVersion7), ikke krevd i request-body.
    public Guid Id { get; set; }
    public required string Name { get; set; }

    // Se IHasUsageMetadata - begge er serverstyrte, aldri satt fra request-body.
    public bool IsSystem { get; set; }
    public int UsageCount { get; set; }
}
