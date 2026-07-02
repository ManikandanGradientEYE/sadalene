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
    public List<UomMaster> AllUoms { get; set; } = [];

    public async Task OnGetAsync()
    {
        SubCategories = await _db.SubCategories
            .Include(s => s.Category).ThenInclude(c => c.Division)
            .Include(s => s.UomMaster)
            .OrderBy(s => s.Category.Division.Name).ThenBy(s => s.Category.Name).ThenBy(s => s.DisplayOrder)
            .ToListAsync();

        AllCategories = await _db.Categories
            .Include(c => c.Division)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Division.Name).ThenBy(c => c.Name)
            .ToListAsync();

        AllUoms = await _db.UomMasters
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(int categoryId, string name, int? uomMasterId, string? description, int displayOrder)
    {
        _db.SubCategories.Add(new SubCategory
        {
            CategoryId   = categoryId,
            Name         = name,
            UomMasterId  = uomMasterId == 0 ? null : uomMasterId,
            Description  = description,
            DisplayOrder = displayOrder
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Sub-Category '{name}' added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, int categoryId, string name, int? uomMasterId, string? description, int displayOrder)
    {
        var s = await _db.SubCategories.FindAsync(id);
        if (s != null)
        {
            s.CategoryId  = categoryId;
            s.Name        = name;
            s.UomMasterId = uomMasterId == 0 ? null : uomMasterId;
            s.Description = description;
            s.DisplayOrder = displayOrder;
            s.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
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
