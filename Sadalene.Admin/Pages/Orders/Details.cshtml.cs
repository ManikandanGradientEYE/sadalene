using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailsModel(ApplicationDbContext db) => _db = db;

    public Order? Order { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Order = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Agent)
            .Include(o => o.PlacedByUser)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Division)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (Order == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status    = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Order status updated to {status}.";
        return RedirectToPage(new { id });
    }
}
