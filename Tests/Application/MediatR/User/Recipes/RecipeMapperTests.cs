using Application.MediatR.User.Recipes;
using Domain.Recipes;
using Xunit;

namespace Tests.Application.MediatR.User.Recipes;

public class RecipeMapperTests
{
    [Fact]
    public void Validate_AcceptsAValidRequest() =>
        Assert.Null(RecipeMapper.Validate(RecipeTestData.ValidRequest()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsABlankTitle(string title) =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { Title = title }));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsABlankDescription(string description) =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { Description = description }));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(RecipeLimits.MaxServings + 1)]
    public void Validate_RejectsServingsOutsideTheAllowedRange(int servings) =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { Servings = servings }));

    [Fact]
    public void Validate_RequiresAtLeastOneStep() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { Steps = [] }));

    [Fact]
    public void Validate_RequiresAtLeastOneIngredient() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { Ingredients = [] }));

    [Fact]
    public void Validate_RejectsABlankStepDescription() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with
        {
            Steps = [new RecipeStepRequest { Description = "  " }]
        }));

    [Fact]
    public void Validate_RejectsANegativeStepTimer() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with
        {
            Steps = [new RecipeStepRequest { Description = "Vent.", TimerMinutes = -5 }]
        }));

    [Fact]
    public void Validate_RejectsALineThatPointsAtBothIngredientKinds() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with
        {
            Ingredients =
            [
                new RecipeIngredientRequest
                {
                    IngredientId = Guid.NewGuid(), UnconfirmedIngredientId = Guid.NewGuid(), UnitId = Guid.NewGuid()
                }
            ]
        }));

    [Fact]
    public void Validate_RejectsALineThatPointsAtNoIngredient() =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { UnitId = Guid.NewGuid() }]
        }));

    [Fact]
    public void Validate_RejectsANegativeAmount_ButAllowsZeroMeaningToTaste()
    {
        var negative = RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { IngredientId = Guid.NewGuid(), Amount = -1, UnitId = Guid.NewGuid() }]
        };
        var toTaste = RecipeTestData.ValidRequest() with
        {
            Ingredients = [new RecipeIngredientRequest { IngredientId = Guid.NewGuid(), Amount = 0, UnitId = Guid.NewGuid() }]
        };

        Assert.NotNull(RecipeMapper.Validate(negative));
        Assert.Null(RecipeMapper.Validate(toTaste));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ikke en adresse")]
    [InlineData("ftp://example.test/bilde.jpg")]
    public void Validate_RejectsAnImageUrlThatIsNotHttpOrHttps(string url) =>
        Assert.NotNull(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { ImageUrl = url }));

    [Fact]
    public void Validate_AcceptsAnHttpsImageUrl() =>
        Assert.Null(RecipeMapper.Validate(RecipeTestData.ValidRequest() with { ImageUrl = "https://example.test/bilde.jpg" }));

    [Fact]
    public void ToRecipe_NormalizesTheTitle_NumbersStepsAndIngredients_AndSumsTheStepTimers()
    {
        var recipeId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = RecipeTestData.ValidRequest() with
        {
            Steps =
            [
                new RecipeStepRequest { Description = " Første ", TimerMinutes = 5 },
                new RecipeStepRequest { Description = "Andre" },
                new RecipeStepRequest { Description = "Tredje", TimerMinutes = 15 }
            ],
            Ingredients =
            [
                new RecipeIngredientRequest { IngredientId = Guid.NewGuid(), Amount = 2, UnitId = Guid.NewGuid() },
                new RecipeIngredientRequest { IngredientId = Guid.NewGuid(), UnitId = Guid.NewGuid(), Note = "  etter smak  " }
            ]
        };

        var recipe = RecipeMapper.ToRecipe(request, recipeId, ownerId, new RecipeSource { Type = RecipeSourceType.Manual },
            false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.Equal("kremet kyllinggryte", recipe.Title);
        Assert.Equal(ownerId, recipe.OwnerUserId);
        Assert.Equal(20, recipe.CookTimeMinutes);
        Assert.Equal([1, 2, 3], recipe.Steps.Select(s => s.StepNumber));
        Assert.Equal("Første", recipe.Steps[0].Description);
        Assert.Equal([1, 2], recipe.Ingredients.Select(i => i.SortOrder));
        Assert.Equal(0m, recipe.Ingredients[1].Amount);
        Assert.Equal("etter smak", recipe.Ingredients[1].Note);
        Assert.All(recipe.Steps, s => Assert.Equal(recipeId, s.RecipeId));
        Assert.All(recipe.Ingredients, i => Assert.Equal(recipeId, i.RecipeId));
        Assert.Equal(3, recipe.Steps.Select(s => s.Id).Distinct().Count());
        Assert.Equal(2, recipe.Ingredients.Select(i => i.Id).Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, recipe.Steps.Select(s => s.Id).Concat(recipe.Ingredients.Select(i => i.Id)));
    }
}
