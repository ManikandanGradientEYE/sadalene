using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Masters.Divisions;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<Division> Divisions { get; set; } = new();
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Divisions.Include(d => d.Categories).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Name.Contains(term));
        }

        Divisions = await query.OrderBy(d => d.Name).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_DivisionsTable", this);

        return Page();
    }

    public async Task<IActionResult> OnPostAddDivisionAsync(string name, string? description)
    {
        _db.Divisions.Add(new Division { Name = name, Description = description ?? string.Empty });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Division '{name}' created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditDivisionAsync(int id, string name, string? description)
    {
        var division = await _db.Divisions.FindAsync(id);
        if (division != null)
        {
            division.Name        = name;
            division.Description = description ?? string.Empty;
            division.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Division updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleDivisionAsync(int id)
    {
        var division = await _db.Divisions.FindAsync(id);
        if (division != null)
        {
            division.IsActive  = !division.IsActive;
            division.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
