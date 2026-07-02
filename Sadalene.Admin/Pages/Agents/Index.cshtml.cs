using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Agents;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public List<Agent> Agents { get; set; } = [];
    public async Task OnGetAsync() =>
        Agents = await _db.Agents.Include(a => a.Orders).OrderBy(a => a.FullName).ToListAsync();
}
