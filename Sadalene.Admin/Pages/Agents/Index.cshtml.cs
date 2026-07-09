using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Agents;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<Agent> Agents { get; set; } = new();
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Agents.Include(a => a.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.FullName.Contains(term) ||
                a.Phone.Contains(term) ||
                (a.AgentCode != null && a.AgentCode.Contains(term)));
        }

        Agents = await query.OrderBy(a => a.FullName).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_AgentsTable", this);

        return Page();
    }
}
