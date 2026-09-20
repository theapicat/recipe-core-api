using Domain.Ingredients;

namespace Persistence.Implementation;

// Flat rad fra get_all_nutrient_definition/get_nutrient_definition_by_id (Dapper matcher kolonnene på navn), som bygges om til
// NutrientDefinition med gruppen (og overordnet gruppe) nøstet inni.
public class NutrientDefinitionRow
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required Guid UnitId { get; set; }
    public required string Unit { get; set; }
    public required Guid UnitTypeId { get; set; }
    public required int DecimalPrecision { get; set; }
    public string? SourceUrl { get; set; }
    public required int SortOrder { get; set; }
    public required Guid GroupId { get; set; }
    public required string GroupName { get; set; }
    public required int GroupSortOrder { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public int? ParentGroupSortOrder { get; set; }

    public NutrientDefinition ToNutrientDefinition() => new()
    {
        Id = Id,
        Name = Name,
        UnitId = UnitId,
        Unit = Unit,
        UnitTypeId = UnitTypeId,
        DecimalPrecision = DecimalPrecision,
        SourceUrl = SourceUrl,
        SortOrder = SortOrder,
        Group = new NutrientGroup
        {
            Id = GroupId,
            Name = GroupName,
            SortOrder = GroupSortOrder,
            ParentGroup = ParentGroupId is { } parentId
                ? new NutrientGroup { Id = parentId, Name = ParentGroupName!, SortOrder = ParentGroupSortOrder!.Value }
                : null
        }
    };
}
