using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Category> Categories { get; set; } = [];
    public List<Division> AllDivisions { get; set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories
            .Include(c => c.Division)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.Division.Name)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        AllDivisions = await _db.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(int divisionId, string name, string? description, int displayOrder)
    {
        _db.Categories.Add(new Category
        {
            DivisionId   = divisionId,
            Name         = name,
            Description  = description,
            DisplayOrder = displayOrder
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Category '{name}' added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, int divisionId, string name, string? description, int displayOrder)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c != null)
        {
            c.DivisionId   = divisionId;
            c.Name         = name;
            c.Description  = description;
            c.DisplayOrder = displayOrder;
            c.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Category updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c != null)
        {
            c.IsActive  = !c.IsActive;
            c.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
