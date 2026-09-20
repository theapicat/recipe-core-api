using Application.Results;
using Domain.Recipes;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class CreateRecipeCommandHandler(
    IRecipeReader reader,
    IRecipeWriter writer,
    IUnconfirmedIngredientReader unconfirmedReader,
    TimeProvider timeProvider)
    : IRequestHandler<CreateRecipeCommand, Result<Recipe>>
{
    public async Task<Result<Recipe>> Handle(CreateRecipeCommand command, CancellationToken cancellationToken)
    {
        var error = RecipeMapper.Validate(command.Request);
        if (error is not null)
            return Result<Recipe>.Invalid(error);

        if (await reader.CountByOwnerAsync(command.UserId) >= RecipeLimits.MaxPerUser)
            return Result<Recipe>.Conflict($"Du kan ha maks {RecipeLimits.MaxPerUser} oppskrifter. Slett noen før du legger til flere.");

        var unconfirmedError = await UnconfirmedIngredientCheck.ValidateAsync(unconfirmedReader, command.UserId, command.Request);
        if (unconfirmedError is not null)
            return Result<Recipe>.Invalid(unconfirmedError);

        // Oppskrifter brukeren lager er alltid Manual. Skrapede oppskrifter (Scraped, med låst url) opprettes av backend når
        // scraper-tjenesten leverer dem - aldri via dette endepunktet.
        var source = new RecipeSource
        {
            Type = RecipeSourceType.Manual,
            Reference = RecipeMapper.NullIfBlank(command.Request.Source?.Reference)
        };
        var now = timeProvider.GetUtcNow();
        var recipe = RecipeMapper.ToRecipe(command.Request, Guid.CreateVersion7(), command.UserId, source, isFavorite: false, now, now);

        await writer.AddAsync(recipe);

        // Leses tilbake så svaret har ingrediensnavnene (fra databasen) med.
        return Result<Recipe>.Success(await reader.GetByIdAsync(recipe.Id, command.UserId) ?? recipe);
    }
}
