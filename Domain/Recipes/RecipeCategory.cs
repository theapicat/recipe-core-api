namespace Domain.Recipes;

public class RecipeCategory : IHasId<Guid>, IHasName
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
