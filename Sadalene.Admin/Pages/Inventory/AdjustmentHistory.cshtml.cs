using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Inventory;

public class AdjustmentHistoryModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    public AdjustmentHistoryModel(ApplicationDbContext db) => _db = db;

    public PagedResult<InventoryAdjustmentLog> Logs { get; set; } = new();
    public string? Search { get; set; }
    public int? DivisionId { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public SelectList Divisions { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? search, int? divisionId, int? categoryId, int? subCategoryId, int pageNumber = 1)
    {
        Search = search;
        DivisionId = divisionId;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;

        var query = _db.InventoryAdjustmentLogs
            .Include(a => a.Product).ThenInclude(p => p.Division)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Product.Name.Contains(term) ||
                (a.Product.ProductCode != null && a.Product.ProductCode.Contains(term)));
        }

        if (divisionId.HasValue) query = query.Where(a => a.Product.DivisionId == divisionId);
        if (categoryId.HasValue) query = query.Where(a => a.Product.CategoryId == categoryId);
        if (subCategoryId.HasValue) query = query.Where(a => a.Product.SubCategoryId == subCategoryId);

        Logs = await query
            .OrderByDescending(a => a.AdjustedAt)
            .ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_AdjustmentHistoryTable", this);

        Divisions = new SelectList(await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

        return Page();
    }
}
