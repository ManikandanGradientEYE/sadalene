using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Inventory;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<InventoryRecord> Inventory { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public List<InventorySyncLog> SyncLogs { get; set; } = [];
    public InventorySyncLog? LastSync { get; set; }

    public async Task OnGetAsync()
    {
        Inventory = await _db.InventoryRecords.Include(i => i.Product).ThenInclude(p => p.Division).OrderBy(i => i.Product.Name).ToListAsync();
        Products  = await _db.Products.Include(p => p.Division).Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
        SyncLogs  = await _db.InventorySyncLogs.OrderByDescending(s => s.SyncStartedAt).Take(10).ToListAsync();
        LastSync  = SyncLogs.FirstOrDefault();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
        var log = new InventorySyncLog { SyncStartedAt = DateTime.UtcNow, TriggeredBy = userName };
        _db.InventorySyncLogs.Add(log);

        try
        {
            // Placeholder — real sync reads from MS Access via ODBC/connection string provided by client
            // For now, mark all existing records as synced
            var records = await _db.InventoryRecords.ToListAsync();
            foreach (var r in records) r.LastSyncedAt = DateTime.UtcNow;

            log.SyncCompletedAt = DateTime.UtcNow;
            log.IsSuccess       = true;
            log.RecordsUpdated  = records.Count;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Sync completed. {records.Count} records updated.";
        }
        catch (Exception ex)
        {
            log.IsSuccess    = false;
            log.ErrorMessage = ex.Message;
            log.SyncCompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Error"] = "Sync failed: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStockAsync(int productId, decimal quantity)
    {
        var record = await _db.InventoryRecords.FirstOrDefaultAsync(r => r.ProductId == productId);
        var product = await _db.Products.Include(p => p.Division).ThenInclude(d => d.UnitOfMeasures)
                                        .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return RedirectToPage();

        var defaultUnit = product.Division.UnitOfMeasures.FirstOrDefault(u => u.IsDefault)?.UnitName
                          ?? product.Division.UnitOfMeasures.FirstOrDefault()?.UnitName ?? "Units";

        if (record == null)
        {
            _db.InventoryRecords.Add(new InventoryRecord { ProductId = productId, QuantityAvailable = quantity, UnitOfMeasure = defaultUnit, LastSyncedAt = DateTime.UtcNow, SyncSource = "Manual" });
        }
        else
        {
            record.QuantityAvailable = quantity;
            record.LastSyncedAt      = DateTime.UtcNow;
            record.SyncSource        = "Manual";
            record.UpdatedAt         = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Stock updated.";
        return RedirectToPage();
    }
}
