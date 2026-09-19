using Domain.Units;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnitWriter(IConfiguration configuration)
    : DbWriter<Unit>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } =
        "SELECT insert_unit(@Id, @Name, @Abbreviation, @UnitTypeId, @BaseUnitRatio);";

    public override string? UpdateCommand { get; } =
        "SELECT update_unit(@Id, @Name, @Abbreviation, @UnitTypeId, @BaseUnitRatio);";

    public override string? DeleteCommand { get; } = "SELECT delete_unit(@Id);";
}
