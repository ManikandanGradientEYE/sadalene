using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Documents;
using Sadalene.Core.Entities.Orders;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Documents.Challans;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public List<Challan> Challans { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<Order> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        Challans  = await _db.Challans.Include(c => c.Customer).Include(c => c.Order).OrderByDescending(c => c.ChallanDate).ToListAsync();
        Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.FullName).ToListAsync();
        Orders    = await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(100).ToListAsync();
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
