using Persistence.Implementation;
using Xunit;

namespace Tests.Persistence.Implementation;

public class NutrientDefinitionRowTests
{
    private static NutrientDefinitionRow Row() => new()
    {
        Id = "Vit C", Name = "Vitamin C (askorbinsyre)", UnitId = Guid.NewGuid(), Unit = "mg", UnitTypeId = Guid.NewGuid(),
        DecimalPrecision = 1, SourceUrl = "https://example.test/vit-c", SortOrder = 42,
        GroupId = Guid.NewGuid(), GroupName = "vitamin c", GroupSortOrder = 10
    };

    [Fact]
    public void ToNutrientDefinition_NestsTheGroup_AndItsParentGroup_WhenTheGroupIsASubgroup()
    {
        var parentId = Guid.NewGuid();
        var row = Row();
        row.ParentGroupId = parentId;
        row.ParentGroupName = "vitaminer";
        row.ParentGroupSortOrder = 7;

        var nutrient = row.ToNutrientDefinition();

        Assert.Equal("Vit C", nutrient.Id);
        Assert.Equal(row.UnitId, nutrient.UnitId);
        Assert.Equal("mg", nutrient.Unit);
        Assert.Equal(row.GroupId, nutrient.Group.Id);
        Assert.Equal("vitamin c", nutrient.Group.Name);
        Assert.Equal(10, nutrient.Group.SortOrder);
        Assert.NotNull(nutrient.Group.ParentGroup);
        Assert.Equal(parentId, nutrient.Group.ParentGroup!.Id);
        Assert.Equal("vitaminer", nutrient.Group.ParentGroup.Name);
        Assert.Equal(7, nutrient.Group.ParentGroup.SortOrder);
        Assert.Null(nutrient.Group.ParentGroup.ParentGroup);
    }

    [Fact]
    public void ToNutrientDefinition_HasNoParentGroup_ForAMainGroup()
    {
        var nutrient = Row().ToNutrientDefinition();

        Assert.Null(nutrient.Group.ParentGroup);
    }
}
