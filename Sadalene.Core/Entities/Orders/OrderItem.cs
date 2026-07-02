using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Orders;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // True when item was added via barcode scan (vs. catalog browse)
    public bool AddedByBarcodeScan { get; set; } = false;
    public string? ScannedBarcodeValue { get; set; }
}
