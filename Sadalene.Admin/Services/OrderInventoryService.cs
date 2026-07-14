using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Services;

/// <summary>
/// Keeps InventoryRecord in sync with order lifecycle: stock is deducted when an order is placed
/// and restored if it's later cancelled (and re-deducted if an already-cancelled order is reinstated).
/// Every change is logged via InventoryAdjustmentLog, same as a manual adjustment, so it shows up
/// in the existing Inventory Adjustment History alongside everything else.
/// </summary>
public class OrderInventoryService
{
    private readonly ApplicationDbContext _db;
    public OrderInventoryService(ApplicationDbContext db) => _db = db;

    public void DeductForOrder(Order order, string adjustedBy) =>
        Apply(order, adjustedBy, restoring: false);

    public void RestoreForOrder(Order order, string adjustedBy) =>
        Apply(order, adjustedBy, restoring: true);

    private void Apply(Order order, string adjustedBy, bool restoring)
    {
        var reason = restoring ? $"Order {order.OrderNumber} cancelled" : $"Order {order.OrderNumber} placed";

        foreach (var item in order.Items)
        {
            var amount = item.EffectiveQuantity;
            if (amount <= 0) continue;

            if (restoring)
                Restore(item.Product, amount, reason, adjustedBy);
            else
                Deduct(item.Product, amount, reason, adjustedBy);
        }
    }

    private void Deduct(Product product, decimal amount, string reason, string adjustedBy)
    {
        var remaining = amount;
        foreach (var record in product.InventoryRecords.OrderByDescending(r => r.QuantityAvailable))
        {
            if (remaining <= 0) break;
            var take = Math.Min(remaining, Math.Max(0, record.QuantityAvailable));
            if (take <= 0) continue;

            Log(record, product.Id, -take, "Subtract", reason, adjustedBy);
            remaining -= take;
        }

        // Defensive: if there wasn't enough spread across existing records (shouldn't happen —
        // stock is validated before the order is saved), still log the shortfall rather than drop it.
        if (remaining > 0)
        {
            var record = product.InventoryRecords.FirstOrDefault();
            if (record != null) Log(record, product.Id, -remaining, "Subtract", reason, adjustedBy);
        }
    }

    private void Restore(Product product, decimal amount, string reason, string adjustedBy)
    {
        var record = product.InventoryRecords.FirstOrDefault();
        if (record == null) return;

        Log(record, product.Id, amount, "Add", reason, adjustedBy);
    }

    private void Log(InventoryRecord record, int productId, decimal delta, string type, string reason, string adjustedBy)
    {
        var previous = record.QuantityAvailable;
        record.QuantityAvailable = Math.Max(0, previous + delta);
        record.LastSyncedAt      = DateTime.UtcNow;
        record.UpdatedAt         = DateTime.UtcNow;
        record.SyncSource        = "Order";

        _db.InventoryAdjustmentLogs.Add(new InventoryAdjustmentLog
        {
            ProductId        = productId,
            AdjustmentType   = type,
            Quantity         = Math.Abs(delta),
            PreviousQuantity = previous,
            NewQuantity      = record.QuantityAvailable,
            Reason           = reason,
            AdjustedBy       = adjustedBy,
            AdjustedAt       = DateTime.UtcNow
        });
    }
}
