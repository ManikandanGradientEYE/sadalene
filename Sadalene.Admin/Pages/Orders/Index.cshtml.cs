using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Orders;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<Order> Orders { get; set; } = new();
    public OrderStatus? StatusFilter { get; set; }
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(OrderStatus? status, string? search, int pageNumber = 1)
    {
        StatusFilter = status;
        Search = search;

        var query = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Agent)
            .Include(o => o.PlacedByUser)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                o.OrderNumber.Contains(term) ||
                o.Customer.FullName.Contains(term) ||
                (o.Agent != null && o.Agent.FullName.Contains(term)));
        }

        Orders = await query.OrderByDescending(o => o.OrderDate).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_OrdersTable", this);

        return Page();
    }
}
