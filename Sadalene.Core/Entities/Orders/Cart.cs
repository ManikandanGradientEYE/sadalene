using Sadalene.Core.Common;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Masters;
using Sadalene.Core.Enums;

namespace Sadalene.Core.Entities.Orders;

/// <summary>
/// A single in-progress order being built via the mobile API. A cart becomes exactly one Order
/// at checkout, so it can only ever hold products from one Division (enforced by CartController
/// on every add, using DivisionId as a cheap check without re-querying Items).
///
/// Ownership — exactly one shape applies per row:
/// - Customer's own cart: CustomerId set, AgentId null, ForCustomerId == CustomerId.
/// - Agent's cart for a customer: AgentId set, CustomerId null, ForCustomerId must be one of Agent.Customers.
/// - Staff/counter cart: both CustomerId and AgentId null. Shared across all staff for that
///   customer (not tied to one staff member) — LastModifiedByUserId is audit-only, not an owner key.
/// </summary>
public class Cart : BaseEntity
{
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public int ForCustomerId { get; set; }
    public Customer ForCustomer { get; set; } = null!;

    /// <summary>Audit-only — last staff member to touch a shared staff/counter cart. Not part of ownership.</summary>
    public int? LastModifiedByUserId { get; set; }
    public User? LastModifiedByUser { get; set; }

    /// <summary>Set from the first item added; cleared when the cart empties. Enforces the single-division rule.</summary>
    public int? DivisionId { get; set; }
    public Division? Division { get; set; }

    public CartStatus Status { get; set; } = CartStatus.Active;

    public ICollection<CartItem> Items { get; set; } = [];
}
