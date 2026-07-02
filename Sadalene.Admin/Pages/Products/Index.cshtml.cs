using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Product> Products { get; set; } = [];

    public async Task OnGetAsync() =>
        Products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Division)
            .Include(p => p.InventoryRecords)
            .OrderBy(p => p.Category.Name).ThenBy(p => p.Name)
            .ToListAsync();
}
