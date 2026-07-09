using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Products;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public PagedResult<Product> Products { get; set; } = new();
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Division)
            .Include(p => p.InventoryRecords)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.ProductCode != null && p.ProductCode.Contains(term)) ||
                p.Category.Name.Contains(term) ||
                p.SubCategory.Name.Contains(term));
        }

        Products = await query
            .OrderBy(p => p.Category.Name).ThenBy(p => p.Name)
            .ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_ProductsTable", this);

        return Page();
    }
}
