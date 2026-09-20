namespace Domain.Recipes;

// Beregnet næring for en oppskrift (svar fra GET /recipes/{id}/nutrition). Lagres ikke - regnes ut på forespørsel fra dagens
// ingrediensdata, så den aldri er utdatert. Veiledende, ikke absolutt.
public class RecipeNutrition
{
    public required Guid RecipeId { get; set; }
    public required int Servings { get; set; }

    // Kun med når summen er større enn 0.
    public NutritionAmount? EnergyKcal { get; set; }
    public NutritionAmount? EnergyKj { get; set; }

    // Kun næringsstoffer med en verdi større enn 0 (ingen nuller, ingen ukjente), sortert som næringsstoff-katalogen. Enheten
    // og navnet slås opp i næringsstoff-katalogen på NutrientId.
    public required List<RecipeNutrientTotal> Nutrients { get; set; }

    // Hvor mange ingredienslinjer som er med i regnestykket av alle linjene - resten står i SkippedLines med årsak.
    public required int CountedIngredients { get; set; }
    public required int TotalIngredients { get; set; }
    public required List<RecipeNutritionSkippedLine> SkippedLines { get; set; }
}

public class NutritionAmount
{
    public required decimal Total { get; set; }
    public required decimal PerServing { get; set; }
}

public class RecipeNutrientTotal
{
    public required string NutrientId { get; set; }
    public required decimal Total { get; set; }
    public required decimal PerServing { get; set; }
}

public class RecipeNutritionSkippedLine
{
    public required Guid RecipeIngredientId { get; set; }
    public required string Name { get; set; }
    public required NutritionSkipReason Reason { get; set; }
}

public enum NutritionSkipReason
{
    // Mengde 0 = «etter smak».
    ToTaste,

    // Brukerens egen ubekreftede ingrediens har ingen næringsdata.
    Unconfirmed,

    // Enheten kan ikke omregnes til gram for denne ingrediensen (f.eks. «klype» uten porsjonsvekt).
    NoConversion
}
