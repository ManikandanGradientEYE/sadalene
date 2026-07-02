using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Orders;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int PendingOrders { get; set; }
    public int ProductsWithBarcode { get; set; }
    public int ProductsWithoutBarcode { get; set; }
    public InventorySyncLog? LastSync { get; set; }
    public List<Order> RecentOrders { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalOrders    = await _db.Orders.CountAsync();
        TotalCustomers = await _db.Customers.CountAsync(c => c.IsActive);
        TotalProducts  = await _db.Products.CountAsync(p => p.IsActive);
        PendingOrders  = await _db.Orders.CountAsync(o => o.Status == Core.Enums.OrderStatus.Pending);

        ProductsWithBarcode    = await _db.Products.CountAsync(p => p.IsActive && p.IsBarcodeActive);
        ProductsWithoutBarcode = await _db.Products.CountAsync(p => p.IsActive && !p.IsBarcodeActive);

        LastSync = await _db.InventorySyncLogs.OrderByDescending(s => s.SyncStartedAt).FirstOrDefaultAsync();

        RecentOrders = await _db.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(8)
            .ToListAsync();
    }
}
