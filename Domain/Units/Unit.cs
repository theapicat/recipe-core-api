namespace Domain.Units;

public class Unit : IHasId<Guid>, IHasName
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Abbreviation { get; set; }
    public required Guid UnitTypeId { get; set; }

    // Forholdstall til denne enhetstypens basisenhet (f.eks. gram for Vekt, milliliter for Volum).
    // Universelt for enheten, ikke avhengig av hvilken ingrediens den brukes på.
    public required decimal BaseUnitRatio { get; set; }
}
