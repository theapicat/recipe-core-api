using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

// Kun liste (til cachen) - enkeltoppslag går mot den fulle Ingredient-modellen via IngredientReader.
public class IngredientListItemReader(IConfiguration configuration)
    : DbReader<IngredientListItem>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_ingredient_list_item();";
}
