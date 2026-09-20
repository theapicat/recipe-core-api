namespace API.Controllers.UserControllers.Recipes;

// Bruker-id er bevisst ikke med - den kommer alltid fra tokenet.
public record SetRecipeFavoriteRequest(bool IsFavorite);
