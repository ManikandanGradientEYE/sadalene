using Sadalene.Core.Common;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Entities.Documents;

namespace Sadalene.Core.Entities.Auth;

/// <summary>
/// Dealers/customers who place orders via the mobile app.
/// Login is OTP-based using phone number.
/// </summary>
public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? GstNumber { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Agent who brought in this customer, if any. Optional — not every customer has an agent.</summary>
    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public ICollection<OtpLog> OtpLogs { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Challan> Challans { get; set; } = [];
}
