using Sadalene.Core.Common;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Core.Entities.Auth;

/// <summary>
/// Agents who bring customers and can place orders on their behalf.
/// Agent login is separate from customer login; both identities are recorded per order.
/// Agents can switch customers without logging out.
/// </summary>
public class Agent : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AgentCode { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<OtpLog> OtpLogs { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
}
