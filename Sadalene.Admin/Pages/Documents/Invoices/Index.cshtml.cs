using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Documents;
using Sadalene.Core.Entities.Orders;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.Admin.Pages.Documents.Invoices;

public class IndexModel : PageModel
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public PagedResult<Invoice> Invoices { get; set; } = new();
    public List<Customer> Customers { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;

        var query = _db.Invoices.Include(i => i.Customer).Include(i => i.Order).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.InvoiceNumber.Contains(term) ||
                i.Customer.FullName.Contains(term));
        }

        Invoices = await query.OrderByDescending(i => i.InvoiceDate).ToPagedResultAsync(pageNumber, PageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Partial("_InvoicesTable", this);

        Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.FullName).ToListAsync();
        Orders    = await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(100).ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(string invoiceNumber, int customerId, int? orderId,
        DateTime invoiceDate, decimal? totalAmount, IFormFile file)
    {
        if (file?.Length > 0)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "invoices");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            _db.Invoices.Add(new Invoice
            {
                InvoiceNumber = invoiceNumber, CustomerId = customerId, OrderId = orderId,
                InvoiceDate = invoiceDate, TotalAmount = totalAmount,
                FileUrl = $"/uploads/invoices/{fileName}"
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Invoice uploaded.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var inv = await _db.Invoices.FindAsync(id);
        if (inv != null) { inv.IsActive = false; inv.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Invoice archived.";
        return RedirectToPage();
    }
}
