using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Order> Orders { get; set; } = [];
    public OrderStatus? StatusFilter { get; set; }

    public async Task OnGetAsync(OrderStatus? status)
    {
        StatusFilter = status;
        var query = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Agent)
            .Include(o => o.PlacedByUser)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        Orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
    }
}
