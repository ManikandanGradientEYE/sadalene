using System.Globalization;
using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Inventory;

[RequestSizeLimit(20_000_000)]
public class UpdateModel : PageModel
{
    // Rows are processed and committed in chunks rather than one giant SaveChanges — bounds memory
    // (change tracker doesn't grow across the whole file) and blast radius (a failure only loses the
    // current chunk, not the entire import), which matters once files run into the tens of thousands of rows.
    private const int BatchSize = 2000;

    private readonly ApplicationDbContext _db;
    public UpdateModel(ApplicationDbContext db) => _db = db;

    public List<string> Errors { get; set; } = [];
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }

    private static readonly string[] RequiredHeaders = ["SKU", "Quantity"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("SKU", true), ("ProductName", false), ("Quantity", true)
    ];

    public int? DivisionId { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public string? DivisionName { get; set; }
    public string? CategoryName { get; set; }
    public string? SubCategoryName { get; set; }

    public async Task OnGetAsync(int? divisionId, int? categoryId, int? subCategoryId)
    {
        DivisionId = divisionId;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;

        if (divisionId.HasValue)
            DivisionName = await _db.Divisions.Where(d => d.Id == divisionId).Select(d => d.Name).FirstOrDefaultAsync();
        if (categoryId.HasValue)
            CategoryName = await _db.Categories.Where(c => c.Id == categoryId).Select(c => c.Name).FirstOrDefaultAsync();
        if (subCategoryId.HasValue)
            SubCategoryName = await _db.SubCategories.Where(s => s.Id == subCategoryId).Select(s => s.Name).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> OnGetTemplateAsync(int? divisionId, int? categoryId, int? subCategoryId)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Inventory");

        for (int i = 0; i < TemplateColumns.Length; i++)
        {
            var (header, required) = TemplateColumns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = required ? $"{header}*" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.FromHtml("#fde8e8") : XLColor.FromHtml("#eef2f7");
        }

        // Pre-fill with the current stock so the file can be edited in place and re-uploaded —
        // rows left unchanged are ignored on import, only actual differences get applied. Scoped to
        // whatever Division/Category/SubCategory filter the user had applied on the Inventory list,
        // so a bulk update can target just one slice of a very large catalog.
        var query = _db.InventoryRecords.Include(i => i.Product).AsQueryable();
        if (divisionId.HasValue) query = query.Where(i => i.Product.DivisionId == divisionId);
        if (categoryId.HasValue) query = query.Where(i => i.Product.CategoryId == categoryId);
        if (subCategoryId.HasValue) query = query.Where(i => i.Product.SubCategoryId == subCategoryId);

        var current = await query
            .OrderBy(i => i.Product.ProductCode)
            .Select(i => new { Sku = i.Product.ProductCode, Name = i.Product.Name, i.QuantityAvailable })
            .ToListAsync();

        int row = 2;
        foreach (var rec in current)
        {
            ws.Cell(row, 1).Value = rec.Sku;
            ws.Cell(row, 2).Value = rec.Name;
            ws.Cell(row, 3).Value = rec.QuantityAvailable;
            row++;
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Inventory-Update-Template.xlsx");
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            Errors.Add("Please choose an Excel file to import.");
            return Page();
        }

        using var upload = new MemoryStream();
        await file.CopyToAsync(upload);
        upload.Position = 0;

        XLWorkbook wb;
        try
        {
            wb = new XLWorkbook(upload);
        }
        catch (Exception)
        {
            Errors.Add("Could not read that file. Please upload a valid .xlsx file (use the downloaded template).");
            return Page();
        }
        using var _ = wb;

        var ws = wb.Worksheet(1);
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in ws.Row(1).CellsUsed())
        {
            var header = cell.GetString().Trim().TrimEnd('*').Trim();
            if (!string.IsNullOrEmpty(header)) colMap[header] = cell.Address.ColumnNumber;
        }

        var missingHeaders = RequiredHeaders.Where(h => !colMap.ContainsKey(h)).ToList();
        if (missingHeaders.Count > 0)
        {
            Errors.Add($"The file is missing required column(s): {string.Join(", ", missingHeaders)}. Please use the downloaded template.");
            return Page();
        }

        string Cell(int row, string header) =>
            colMap.TryGetValue(header, out var col) ? ws.Cell(row, col).GetString().Trim() : "";

        // Pass 1: read every row into memory without touching the database, so we can batch-fetch
        // products by SKU instead of round-tripping per row (the catalog can run into the tens of thousands).
        var rows = new List<(int Row, string Sku, decimal Quantity)>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var sku = Cell(row, "SKU");
            var quantityRaw = Cell(row, "Quantity");

            if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(quantityRaw))
                continue; // fully blank row

            if (string.IsNullOrWhiteSpace(sku)) { Errors.Add($"Row {row}: SKU is required."); continue; }
            if (string.IsNullOrWhiteSpace(quantityRaw)) { Errors.Add($"Row {row}: Quantity is required."); continue; }

            if (!decimal.TryParse(quantityRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) || quantity < 0)
            {
                Errors.Add($"Row {row}: Quantity '{quantityRaw}' is not a valid non-negative number.");
                continue;
            }

            rows.Add((row, sku.Trim().ToUpperInvariant(), quantity));
        }

        if (rows.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No inventory rows found in the uploaded file.");
            return Page();
        }

        var adjustedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

        foreach (var batch in rows.Chunk(BatchSize))
        {
            var skus = batch.Select(r => r.Sku).Distinct().ToList();
            var productsBySku = await _db.Products
                .Include(p => p.InventoryRecords)
                .Where(p => p.ProductCode != null && skus.Contains(p.ProductCode))
                .ToDictionaryAsync(p => p.ProductCode!, StringComparer.OrdinalIgnoreCase);

            var batchUpdated = 0;

            foreach (var (row, sku, quantity) in batch)
            {
                if (!productsBySku.TryGetValue(sku, out var product))
                {
                    Errors.Add($"Row {row}: SKU '{sku}' was not found.");
                    continue;
                }

                var record = product.InventoryRecords.FirstOrDefault();
                if (record == null)
                {
                    Errors.Add($"Row {row}: SKU '{sku}' has no inventory record to update.");
                    continue;
                }

                if (record.QuantityAvailable == quantity)
                {
                    UnchangedCount++;
                    continue;
                }

                var previous = record.QuantityAvailable;
                record.QuantityAvailable = quantity;
                record.LastSyncedAt      = DateTime.UtcNow;
                record.SyncSource        = "Import";
                record.UpdatedAt         = DateTime.UtcNow;

                _db.InventoryAdjustmentLogs.Add(new InventoryAdjustmentLog
                {
                    ProductId        = product.Id,
                    AdjustmentType   = "Set",
                    Quantity         = quantity,
                    PreviousQuantity = previous,
                    NewQuantity      = quantity,
                    Reason           = "Bulk update via Excel import",
                    AdjustedBy       = adjustedBy,
                    AdjustedAt       = DateTime.UtcNow
                });

                batchUpdated++;
            }

            if (batchUpdated > 0)
            {
                try
                {
                    await _db.SaveChangesAsync();
                    UpdatedCount += batchUpdated;
                }
                catch (DbUpdateException ex)
                {
                    Errors.Add($"Rows {batch[0].Row}–{batch[^1].Row}: this batch of {batchUpdated} change(s) failed to save and was not applied. " +
                        $"Details: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // Detach everything from this chunk before the next one — keeps the change tracker
            // (and its DetectChanges cost) from growing across the whole file.
            _db.ChangeTracker.Clear();
        }

        TempData["Success"] = $"{UpdatedCount} SKU(s) updated, {UnchangedCount} already matched and were left unchanged.";
        return Page();
    }
}
