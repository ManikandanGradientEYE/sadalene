using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

public class BarcodeModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public BarcodeModel(ApplicationDbContext db) => _db = db;

    public Product? Product { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Division)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (Product == null) return NotFound();
        return Page();
    }
}
