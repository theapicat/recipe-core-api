using Domain.Recipes;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class RecipeCategoryWriter(IConfiguration configuration)
    : DbWriter<RecipeCategory>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } = "SELECT insert_recipe_category(@Id, @Name);";
    public override string? UpdateCommand { get; } = "SELECT update_recipe_category(@Id, @Name);";
    public override string? DeleteCommand { get; } = "SELECT delete_recipe_category(@Id);";
}
