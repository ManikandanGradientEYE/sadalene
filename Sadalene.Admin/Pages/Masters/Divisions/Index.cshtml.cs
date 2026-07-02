using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.Divisions;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Division> Divisions { get; set; } = [];

    public async Task OnGetAsync() =>
        Divisions = await _db.Divisions
            .Include(d => d.Categories)
            .OrderBy(d => d.Name)
            .ToListAsync();

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
