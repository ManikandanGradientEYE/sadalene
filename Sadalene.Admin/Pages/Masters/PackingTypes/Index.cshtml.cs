using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Masters.PackingTypes;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<PackingType> PackingTypes { get; set; } = new();
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.PackingTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        PackingTypes = await query.OrderBy(p => p.Name).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_PackingTypesTable", this);

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name, string? description)
    {
        if (await _db.PackingTypes.AnyAsync(p => p.Name == name))
        {
            TempData["Error"] = $"Packing type '{name}' already exists.";
            return RedirectToPage();
        }
        _db.PackingTypes.Add(new PackingType { Name = name, Description = description });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Packing type '{name}' added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description)
    {
        var p = await _db.PackingTypes.FindAsync(id);
        if (p != null) { p.Name = name; p.Description = description; p.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Packing type updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var p = await _db.PackingTypes.FindAsync(id);
        if (p != null) { p.IsActive = !p.IsActive; p.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
