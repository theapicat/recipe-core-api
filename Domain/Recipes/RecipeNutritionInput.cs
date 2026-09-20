namespace Domain.Recipes;

// Rådata til næringsberegningen for én oppskrift, hentet i ett kall til databasen (kun for eierens egen oppskrift). Selve
// regnestykket ligger i Application (RecipeNutritionCalculator) og er rent, så det kan enhetstestes uten database.
public class RecipeNutritionInput
{
    public required Guid RecipeId { get; set; }
    public required int Servings { get; set; }
    public required List<RecipeNutritionLine> Lines { get; set; }
    public required List<RecipeNutritionPortion> Portions { get; set; }
    public required List<RecipeNutritionValue> Values { get; set; }
}

// Én ingredienslinje i oppskriften med enheten den er oppgitt i. IngredientId er null for en ubekreftet ingrediens (ingen næringsdata).
public class RecipeNutritionLine
{
    public required Guid LineId { get; set; }
    public required int SortOrder { get; set; }
    public required string Name { get; set; }
    public Guid? IngredientId { get; set; }
    public required decimal Amount { get; set; }
    public required Guid UnitId { get; set; }
    public required string UnitTypeName { get; set; }
    public required decimal UnitBaseRatio { get; set; }

    // Per 100 g spiselig del.
    public decimal? EnergyKcal { get; set; }
    public decimal? EnergyKj { get; set; }

    // Andel av matvaren som er spiselig. Null = ukjent (regnes som 100).
    public decimal? EdiblePartPercent { get; set; }
}

// Ingrediensens egen enhet -> gram-omregning. Vekten er vekt av SPISELIG del (Matvaretabellens porsjonsvekter).
public class RecipeNutritionPortion
{
    public required Guid IngredientId { get; set; }
    public required Guid UnitId { get; set; }
    public required string UnitTypeName { get; set; }
    public required decimal UnitBaseRatio { get; set; }
    public required decimal GramsPerPortion { get; set; }
}

// Målt verdi per 100 g spiselig del (sparsom).
public class RecipeNutritionValue
{
    public required Guid IngredientId { get; set; }
    public required string NutrientDefinitionId { get; set; }
    public required decimal Quantity { get; set; }
    public required int NutrientSortOrder { get; set; }
}
