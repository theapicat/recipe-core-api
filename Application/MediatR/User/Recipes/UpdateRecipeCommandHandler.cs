using Application.Results;
using Domain.Recipes;
using MediatR;
using Persistence.Interfaces;

namespace Application.MediatR.User.Recipes;

public class UpdateRecipeCommandHandler(
    IRecipeReader reader,
    IRecipeWriter writer,
    IUnconfirmedIngredientReader unconfirmedReader,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateRecipeCommand, Result<Recipe>>
{
    public async Task<Result<Recipe>> Handle(UpdateRecipeCommand command, CancellationToken cancellationToken)
    {
        var error = RecipeMapper.Validate(command.Request);
        if (error is not null)
            return Result<Recipe>.Invalid(error);

        var existing = await reader.GetByIdAsync(command.Id, command.UserId);
        if (existing is null)
            return Result<Recipe>.NotFound();

        var unconfirmedError = await UnconfirmedIngredientCheck.ValidateAsync(unconfirmedReader, command.UserId, command.Request);
        if (unconfirmedError is not null)
            return Result<Recipe>.Invalid(unconfirmedError);

        // Kilde-type og url er låst. Fritekstreferansen kan endres, og redigerer brukeren en skrapet oppskrift markeres den som
        // endret fra kilden.
        var source = new RecipeSource
        {
            Type = existing.Source.Type,
            Reference = RecipeMapper.NullIfBlank(command.Request.Source?.Reference),
            Url = existing.Source.Url,
            IsEditedFromSource = existing.Source.Type == RecipeSourceType.Scraped ? true : existing.Source.IsEditedFromSource
        };
        var recipe = RecipeMapper.ToRecipe(command.Request, existing.Id, command.UserId, source, existing.IsFavorite,
            existing.CreatedAt, timeProvider.GetUtcNow());

        if (!await writer.UpdateAsync(recipe))
            return Result<Recipe>.NotFound();

        return Result<Recipe>.Success(await reader.GetByIdAsync(recipe.Id, command.UserId) ?? recipe);
    }
}
