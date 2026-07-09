using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Masters.ProductTypes;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<ProductType> Types { get; set; } = new();
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.ProductTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        Types = await query.OrderBy(t => t.Name).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_ProductTypesTable", this);

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name, string? description)
    {
        name = name.Trim();

        if (await _db.ProductTypes.AnyAsync(t => t.Name == name))
        {
            TempData["Error"] = $"A product type named '{name}' already exists.";
            return RedirectToPage();
        }

        _db.ProductTypes.Add(new ProductType { Name = name, Description = description });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product type added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description)
    {
        name = name.Trim();

        if (await _db.ProductTypes.AnyAsync(t => t.Name == name && t.Id != id))
        {
            TempData["Error"] = $"A product type named '{name}' already exists.";
            return RedirectToPage();
        }

        var t = await _db.ProductTypes.FindAsync(id);
        if (t != null) { t.Name = name; t.Description = description; t.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Product type updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var t = await _db.ProductTypes.FindAsync(id);
        if (t != null) { t.IsActive = !t.IsActive; t.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
