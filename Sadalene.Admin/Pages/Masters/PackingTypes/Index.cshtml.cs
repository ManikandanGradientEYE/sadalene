using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.PackingTypes;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<PackingType> PackingTypes { get; set; } = [];

    public async Task OnGetAsync() =>
        PackingTypes = await _db.PackingTypes.OrderBy(p => p.Name).ToListAsync();

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
