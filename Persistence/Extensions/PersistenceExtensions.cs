using Dapper;
using Domain.Ingredients;
using Domain.Recipes;
using Domain.Units;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Implementation;
using Persistence.Services;

namespace Persistence.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        // Postgres-kolonner er snake_case, C#-egenskaper er PascalCase - dette lar Dapper matche dem
        // automatisk (owner_user_id -> OwnerUserId) uten kolonne-alias i hver spørring.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddScoped<DbReader<IngredientCategory>, IngredientCategoryReader>();
        services.AddScoped<DbWriter<IngredientCategory>, IngredientCategoryWriter>();

        services.AddScoped<DbReader<Allergen>, AllergenReader>();
        services.AddScoped<DbWriter<Allergen>, AllergenWriter>();

        services.AddScoped<DbReader<SearchKeyword>, SearchKeywordReader>();
        services.AddScoped<DbWriter<SearchKeyword>, SearchKeywordWriter>();

        services.AddScoped<DbReader<UnitType>, UnitTypeReader>();
        services.AddScoped<DbWriter<UnitType>, UnitTypeWriter>();

        services.AddScoped<DbReader<Unit>, UnitReader>();
        services.AddScoped<DbWriter<Unit>, UnitWriter>();

        services.AddScoped<DbReader<RecipeCategory>, RecipeCategoryReader>();
        services.AddScoped<DbWriter<RecipeCategory>, RecipeCategoryWriter>();

        return services;
    }
}
