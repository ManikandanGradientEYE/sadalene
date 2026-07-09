using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Masters.SubCategories;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<SubCategory> SubCategories { get; set; } = new();
    public List<Category> AllCategories { get; set; } = [];
    public List<UomMaster> AllUoms { get; set; } = [];
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.SubCategories
            .Include(s => s.Category).ThenInclude(c => c.Division)
            .Include(s => s.UomMaster)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.Name.Contains(term) ||
                s.Category.Name.Contains(term));
        }

        SubCategories = await query
            .OrderBy(s => s.Category.Division.Name).ThenBy(s => s.Category.Name).ThenBy(s => s.DisplayOrder)
            .ToPagedResultAsync(pageNumber, PageSize);

        // Needed by the per-row edit modal, which is rendered inside the AJAX-swapped table partial too.
        AllCategories = await _db.Categories
            .Include(c => c.Division)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Division.Name).ThenBy(c => c.Name)
            .ToListAsync();

        AllUoms = await _db.UomMasters
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_SubCategoriesTable", this);

        return Page();
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
