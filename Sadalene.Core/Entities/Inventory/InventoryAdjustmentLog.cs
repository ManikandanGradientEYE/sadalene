using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Inventory;

public class InventoryAdjustmentLog
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string AdjustmentType { get; set; } = string.Empty;  // "Add", "Subtract", "Set"
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? Reason { get; set; }
    public string AdjustedBy { get; set; } = string.Empty;
    public DateTime AdjustedAt { get; set; } = DateTime.UtcNow;
}
