namespace API.Controllers.UserControllers.UnconfirmedIngredients;

// Bruker-id er bevisst ikke med - den kommer alltid fra tokenet.
public record CreateUnconfirmedIngredientRequest(string Name, bool RequestReview = false);

public record RenameUnconfirmedIngredientRequest(string Name);
