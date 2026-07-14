using Sadalene.Core.Common;
using Sadalene.Core.Enums;
using Sadalene.Core.Entities.Auth;

namespace Sadalene.Core.Entities.Orders;

public class Order : BaseEntity
{
    /// <summary>
    /// Unique order number shown to dealer immediately after placement (e.g. ORD-2026-000001).
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    // Populated when an agent places the order on behalf of a customer
    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    // Populated when admin staff places order via master login
    public int? PlacedByUserId { get; set; }
    public User? PlacedByUser { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];

    public decimal GrandTotal => Items.Sum(i => i.LineTotal);
}
