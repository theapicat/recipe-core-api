namespace Domain.Ingredients;

public class IngredientCategory : IHasId<Guid>
{
    // Tildeles av serveren ved opprettelse (Guid.CreateVersion7), ikke krevd i request-body.
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
