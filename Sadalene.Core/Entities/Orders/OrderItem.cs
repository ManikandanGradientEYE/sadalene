using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;
using Sadalene.Core.Enums;

namespace Sadalene.Core.Entities.Orders;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Full vs Half only matters for products whose UOM has UomMaster.AllowsHalfUnit set;
    // otherwise it stays Full and has no effect.
    public OrderItemUnitType UnitType { get; set; } = OrderItemUnitType.Full;

    // What this line actually costs in stock: Quantity is a piece count (e.g. "2 Half Lumps"),
    // this is the Lump-equivalent amount (e.g. 1.0) used for stock validation and display.
    public decimal EffectiveQuantity => UnitType == OrderItemUnitType.Half ? Quantity * 0.5m : Quantity;

    // True when item was added via barcode scan (vs. catalog browse)
    public bool AddedByBarcodeScan { get; set; } = false;
    public string? ScannedBarcodeValue { get; set; }
}
