using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Inventory;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<InventoryRecord> Inventory { get; set; } = [];
    public List<InventoryAdjustmentLog> AdjustmentLogs { get; set; } = [];
    public List<InventorySyncLog> SyncLogs { get; set; } = [];
    public InventorySyncLog? LastSync { get; set; }

    public async Task OnGetAsync()
    {
        Inventory = await _db.InventoryRecords
            .Include(i => i.Product).ThenInclude(p => p.Division)
            .Include(i => i.Product).ThenInclude(p => p.UomMaster)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();

        AdjustmentLogs = await _db.InventoryAdjustmentLogs
            .Include(a => a.Product)
            .OrderByDescending(a => a.AdjustedAt)
            .Take(30)
            .ToListAsync();

        SyncLogs = await _db.InventorySyncLogs
            .OrderByDescending(s => s.SyncStartedAt)
            .Take(10)
            .ToListAsync();

        LastSync = SyncLogs.FirstOrDefault();
    }

    public async Task<IActionResult> OnPostAdjustAsync(int productId, string adjustType, decimal quantity, string? reason)
    {
        var record = await _db.InventoryRecords
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ProductId == productId);

        if (record == null)
        {
            TempData["Error"] = "Inventory record not found.";
            return RedirectToPage();
        }

        var previous = record.QuantityAvailable;
        decimal newQty = adjustType switch
        {
            "Add"      => previous + quantity,
            "Subtract" => Math.Max(0, previous - quantity),
            "Set"      => quantity,
            _          => previous
        };

        record.QuantityAvailable = newQty;
        record.LastSyncedAt      = DateTime.UtcNow;
        record.SyncSource        = "Manual";
        record.UpdatedAt         = DateTime.UtcNow;

        _db.InventoryAdjustmentLogs.Add(new InventoryAdjustmentLog
        {
            ProductId        = productId,
            AdjustmentType   = adjustType,
            Quantity         = quantity,
            PreviousQuantity = previous,
            NewQuantity      = newQty,
            Reason           = reason?.Trim(),
            AdjustedBy       = User.FindFirstValue(ClaimTypes.Name) ?? "Admin",
            AdjustedAt       = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var label = adjustType == "Set" ? $"set to {newQty:N2}" : $"{previous:N2} → {newQty:N2}";
        TempData["Success"] = $"{record.Product.Name}: stock {label} {record.UnitOfMeasure}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
        var log = new InventorySyncLog { SyncStartedAt = DateTime.UtcNow, TriggeredBy = userName };
        _db.InventorySyncLogs.Add(log);

        try
        {
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
            log.IsSuccess       = false;
            log.ErrorMessage    = ex.Message;
            log.SyncCompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Error"] = "Sync failed: " + ex.Message;
        }

        return RedirectToPage();
    }
}
