namespace Domain.Units;

public class UnitType : IHasId<Guid>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
