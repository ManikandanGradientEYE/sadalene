using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Masters.Categories;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<Category> Categories { get; set; } = new();
    public List<Division> AllDivisions { get; set; } = [];
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Categories
            .Include(c => c.Division)
            .Include(c => c.SubCategories)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Division.Name.Contains(term));
        }

        Categories = await query
            .OrderBy(c => c.Division.Name).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .ToPagedResultAsync(pageNumber, PageSize);

        // Needed by the per-row edit modal, which is rendered inside the AJAX-swapped table partial too.
        AllDivisions = await _db.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_CategoriesTable", this);

        return Page();
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
