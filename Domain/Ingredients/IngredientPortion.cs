namespace Domain.Ingredients;

// Enhet -> gram-konvertering per ingrediens (speiler Matvaretabellens "portions"-data),
// i stedet for en generell tetthetsberegning.
public class IngredientPortion
{
    public required Guid Id { get; set; }
    public required Guid IngredientId { get; set; }
    public required Guid UnitId { get; set; }
    public required decimal GramsPerPortion { get; set; }
}
