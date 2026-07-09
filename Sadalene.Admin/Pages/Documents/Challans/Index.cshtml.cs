using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Documents;
using Sadalene.Core.Entities.Orders;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Documents.Challans;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public PagedResult<Challan> Challans { get; set; } = new();
    public List<Customer> Customers { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Challans.Include(c => c.Customer).Include(c => c.Order).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.ChallanNumber.Contains(term) ||
                c.Customer.FullName.Contains(term));
        }

        Challans = await query.OrderByDescending(c => c.ChallanDate).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_ChallansTable", this);

        Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.FullName).ToListAsync();
        Orders    = await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(100).ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(string challanNumber, int customerId, int? orderId,
        DateTime challanDate, IFormFile file)
    {
        if (file?.Length > 0)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "challans");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            _db.Challans.Add(new Challan
            {
                ChallanNumber = challanNumber, CustomerId = customerId, OrderId = orderId,
                ChallanDate = challanDate, FileUrl = $"/uploads/challans/{fileName}"
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Challan uploaded.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var c = await _db.Challans.FindAsync(id);
        if (c != null) { c.IsActive = false; c.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Challan archived.";
        return RedirectToPage();
    }
}
