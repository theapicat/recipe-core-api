using Domain.Recipes;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class RecipeCategoryReader(IConfiguration configuration)
    : DbReader<RecipeCategory>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_recipe_category();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_recipe_category_by_id(@Id);";
}
