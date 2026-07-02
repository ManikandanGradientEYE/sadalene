using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.ProductTypes;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<ProductType> Types { get; set; } = [];

    public async Task OnGetAsync() =>
        Types = await _db.ProductTypes.OrderBy(t => t.Name).ToListAsync();

    public async Task<IActionResult> OnPostAddAsync(string name, string? description)
    {
        _db.ProductTypes.Add(new ProductType { Name = name, Description = description });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product type added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name, string? description)
    {
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
