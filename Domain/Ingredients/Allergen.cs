namespace Domain.Ingredients;

public class Allergen : IHasId<Guid>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
