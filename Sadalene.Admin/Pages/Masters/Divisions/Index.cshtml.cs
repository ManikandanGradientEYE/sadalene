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
        Divisions = await _db.Divisions.Include(d => d.UnitOfMeasures).OrderBy(d => d.Name).ToListAsync();

    public async Task<IActionResult> OnPostAddUnitAsync(int divisionId, string unitName, bool isDefault)
    {
        _db.DivisionUnitOfMeasures.Add(new DivisionUnitOfMeasure
        {
            DivisionId = divisionId,
            UnitName   = unitName,
            IsDefault  = isDefault
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Unit of measure added.";
        return RedirectToPage();
    }
}
