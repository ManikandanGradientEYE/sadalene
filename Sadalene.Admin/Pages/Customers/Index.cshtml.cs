using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Customer> Customers { get; set; } = [];

    public async Task OnGetAsync() =>
        Customers = await _db.Customers.Include(c => c.Orders).Include(c => c.Agent).OrderBy(c => c.FullName).ToListAsync();
}
