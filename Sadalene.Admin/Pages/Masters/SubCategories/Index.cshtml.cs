using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.SubCategories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<SubCategory> SubCategories { get; set; } = [];
    public List<Category> AllCategories { get; set; } = [];

    public async Task OnGetAsync()
    {
        SubCategories = await _db.SubCategories.Include(s => s.Category).OrderBy(s => s.Category.Name).ThenBy(s => s.DisplayOrder).ToListAsync();
        AllCategories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(int categoryId, string name, string? description, int displayOrder)
    {
        _db.SubCategories.Add(new SubCategory { CategoryId = categoryId, Name = name, Description = description, DisplayOrder = displayOrder });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sub-Category added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, int categoryId, string name, string? description, int displayOrder)
    {
        var s = await _db.SubCategories.FindAsync(id);
        if (s != null) { s.CategoryId = categoryId; s.Name = name; s.Description = description; s.DisplayOrder = displayOrder; s.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Sub-Category updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var s = await _db.SubCategories.FindAsync(id);
        if (s != null) { s.IsActive = !s.IsActive; s.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
