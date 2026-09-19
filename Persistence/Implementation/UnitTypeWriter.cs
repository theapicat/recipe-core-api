using Domain.Units;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Persistence.Implementation;

public class UnitTypeWriter(IConfiguration configuration)
    : DbWriter<UnitType>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? InsertCommand { get; } = "SELECT insert_unit_type(@Id, @Name);";
    public override string? UpdateCommand { get; } = "SELECT update_unit_type(@Id, @Name);";
    public override string? DeleteCommand { get; } = "SELECT delete_unit_type(@Id);";
}
