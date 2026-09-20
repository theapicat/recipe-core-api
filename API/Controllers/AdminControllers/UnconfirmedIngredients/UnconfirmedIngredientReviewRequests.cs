namespace API.Controllers.AdminControllers.UnconfirmedIngredients;

public record MergeUnconfirmedIngredientRequest(Guid IngredientId);

public record RejectUnconfirmedIngredientRequest(string? Reason);
