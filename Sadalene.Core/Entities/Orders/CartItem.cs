using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;
using Sadalene.Core.Enums;

namespace Sadalene.Core.Entities.Orders;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Full vs Half only matters for products whose UOM has UomMaster.AllowsHalfUnit set;
    // otherwise it stays Full and has no effect. Same semantics as OrderItem.UnitType.
    public OrderItemUnitType UnitType { get; set; } = OrderItemUnitType.Full;

    public decimal EffectiveQuantity => UnitType == OrderItemUnitType.Half ? Quantity * 0.5m : Quantity;

    // Display-only snapshot of Product.Rate at add-time, so the app can show a running total
    // without a live join. NEVER trusted for money math — checkout re-fetches the live Rate and
    // snapshots it onto the real OrderItem.UnitPrice at that moment, since a cart can sit for days.
    public decimal DisplayUnitPrice { get; set; }

    public bool AddedByBarcodeScan { get; set; } = false;
    public string? ScannedBarcodeValue { get; set; }
}
