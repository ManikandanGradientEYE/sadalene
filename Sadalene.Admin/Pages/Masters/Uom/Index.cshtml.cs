using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.Uom;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<UomMaster> UomList { get; set; } = [];

    public async Task OnGetAsync() =>
        UomList = await _db.UomMasters
            .OrderBy(u => u.Name)
            .ToListAsync();

    public async Task<IActionResult> OnPostAddAsync(string name, string? abbreviation, string? description)
    {
        if (await _db.UomMasters.AnyAsync(u => u.Name == name))
        {
            TempData["Error"] = $"UOM '{name}' already exists.";
            return RedirectToPage();
        }
        _db.UomMasters.Add(new UomMaster { Name = name, Abbreviation = abbreviation, Description = description });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"UOM '{name}' added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string? abbreviation, string? description)
    {
        var u = await _db.UomMasters.FindAsync(id);
        if (u != null)
        {
            u.Name         = name;
            u.Abbreviation = abbreviation;
            u.Description  = description;
            u.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "UOM updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var u = await _db.UomMasters.FindAsync(id);
        if (u != null) { u.IsActive = !u.IsActive; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
