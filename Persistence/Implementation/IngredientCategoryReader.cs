using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class IngredientCategoryReader(IConfiguration configuration)
    : DbReader<IngredientCategory>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_ingredient_category();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_ingredient_category_by_id(@Id);";
}
