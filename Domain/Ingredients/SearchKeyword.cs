namespace Domain.Ingredients;

public class SearchKeyword : IHasId<Guid>, IHasName
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
