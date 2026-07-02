using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Customers;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailsModel(ApplicationDbContext db) => _db = db;

    public Customer? Customer { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Customer = await _db.Customers
            .Include(c => c.Orders).ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (Customer == null) return NotFound();
        return Page();
    }
}
