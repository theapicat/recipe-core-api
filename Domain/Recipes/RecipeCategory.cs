namespace Domain.Recipes;

public class RecipeCategory : IHasId<Guid>, IHasName, IHasUsageMetadata
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    // Se IHasUsageMetadata - begge er serverstyrte, aldri satt fra request-body.
    public bool IsSystem { get; set; }
    public int UsageCount { get; set; }
}
