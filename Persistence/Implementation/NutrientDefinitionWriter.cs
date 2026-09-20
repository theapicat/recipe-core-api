using Domain.Ingredients;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class NutrientDefinitionWriter(IConfiguration configuration)
    : DbWriter<NutrientDefinition>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } =
        "SELECT insert_nutrient_definition(@Id, @Name, @Unit, @DecimalPrecision, @ParentId, @SourceUrl);";

    public override string? UpdateCommand { get; } =
        "SELECT update_nutrient_definition(@Id, @Name, @Unit, @DecimalPrecision, @ParentId, @SourceUrl);";

    public override string? DeleteCommand { get; } = "SELECT delete_nutrient_definition(@Id);";
}
