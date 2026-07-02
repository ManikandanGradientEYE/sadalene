using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Inventory;

/// <summary>
/// Current stock level for a product. Updated via manual or scheduled sync from MS Access.
/// </summary>
public class InventoryRecord : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal QuantityAvailable { get; set; } = 0;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public string? SyncSource { get; set; }
}
