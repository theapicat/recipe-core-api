namespace Domain.Ingredients;

public class SearchKeyword : IHasId<Guid>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
