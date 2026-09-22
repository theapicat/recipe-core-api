namespace Domain.Units;

// Navnene på enhetstypene beregningene i backend forholder seg til (næringsberegning av oppskrifter). Verdiene er de som seedes
// (seed_01_units). Admin bør ikke gi disse enhetstypene nytt navn - da vil linjer i den enhetstypen bli rapportert som «kan ikke omregnes»
// i stedet for å gi feil tall.
public static class UnitTypeNames
{
    public const string Weight = "vekt";
    public const string Volume = "volum";
    public const string Count = "antall";
}
