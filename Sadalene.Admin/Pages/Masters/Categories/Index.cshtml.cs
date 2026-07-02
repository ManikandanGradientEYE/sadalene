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

    public async Task OnGetAsync() =>
        Categories = await _db.Categories.Include(c => c.SubCategories).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync();

    public async Task<IActionResult> OnPostAddAsync(string name, string? description, int displayOrder)
    {
        _db.Categories.Add(new Category { Name = name, Description = description, DisplayOrder = displayOrder });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Category added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description, int displayOrder)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c != null) { c.Name = name; c.Description = description; c.DisplayOrder = displayOrder; c.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Category updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c != null) { c.IsActive = !c.IsActive; c.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
