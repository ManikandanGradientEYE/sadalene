using Sadalene.Core.Common;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Core.Entities.Documents;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public string FileUrl { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal? TotalAmount { get; set; }
}
