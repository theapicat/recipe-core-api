using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class IngredientCategoryWriter(IConfiguration configuration)
    : DbWriter<IngredientCategory>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } = "SELECT insert_ingredient_category(@Id, @Name);";
    public override string? UpdateCommand { get; } = "SELECT update_ingredient_category(@Id, @Name);";
    public override string? DeleteCommand { get; } = "SELECT delete_ingredient_category(@Id);";
}
