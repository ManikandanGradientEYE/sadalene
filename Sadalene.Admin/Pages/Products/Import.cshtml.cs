using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Masters;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

[RequestSizeLimit(20_000_000)]
public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ImportModel(ApplicationDbContext db) => _db = db;

    public List<string> Errors { get; set; } = [];
    public int ImportedCount { get; set; }

    // Column headers used by both the downloadable template and the row parser.
    private static readonly string[] RequiredHeaders = ["Division", "Category", "SubCategory", "ProductType", "Name"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("Division", true), ("Category", true), ("SubCategory", true), ("ProductType", true), ("Name", true),
        ("ProductCode", false), ("MarketName", false), ("Description", false),
        ("UOM", false), ("PackingType", false),
        ("Rate", false), ("RatePer", false), ("Cut", false), ("QtyPerUnit", false), ("Grade", false),
        ("FabricComposition", false), ("Width", false), ("Weight", false), ("Color", false),
        ("DesignNo", false), ("Design", false), ("Brand", false), ("InitialStock", false)
    ];

    public void OnGet() { }

    public async Task<IActionResult> OnGetTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Products");

        for (int i = 0; i < TemplateColumns.Length; i++)
        {
            var (header, required) = TemplateColumns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = required ? $"{header}*" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.FromHtml("#fde8e8") : XLColor.FromHtml("#eef2f7");
        }
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        var refWs = wb.Worksheets.Add("Reference (existing names)");
        var refHeaders = new[] { "Divisions", "Categories (Division / Category)", "SubCategories (Category / SubCategory)", "Product Types", "Packing Types", "UOMs" };
        for (int i = 0; i < refHeaders.Length; i++)
        {
            refWs.Cell(1, i + 1).Value = refHeaders[i];
            refWs.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var divisions = await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name)
            .Select(d => d.Code != null ? $"{d.Name} ({d.Code})" : d.Name).ToListAsync();
        var categories = await _db.Categories.Where(c => c.IsActive).Include(c => c.Division)
            .OrderBy(c => c.Division.Name).ThenBy(c => c.Name)
            .Select(c => c.Division.Name + " / " + c.Name).ToListAsync();
        var subCategories = await _db.SubCategories.Where(s => s.IsActive).Include(s => s.Category)
            .OrderBy(s => s.Category.Name).ThenBy(s => s.Name)
            .Select(s => s.Category.Name + " / " + s.Name).ToListAsync();
        var productTypes = await _db.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();
        var packingTypes = await _db.PackingTypes.Where(p => p.IsActive).OrderBy(p => p.Name).Select(p => p.Name).ToListAsync();
        var uoms = await _db.UomMasters.Where(u => u.IsActive).OrderBy(u => u.Name).Select(u => u.Name).ToListAsync();

        var lists = new[] { divisions, categories, subCategories, productTypes, packingTypes, uoms };
        for (int col = 0; col < lists.Length; col++)
            for (int i = 0; i < lists[col].Count; i++)
                refWs.Cell(i + 2, col + 1).Value = lists[col][i];
        refWs.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Product-Import-Template.xlsx");
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
        static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
        static decimal? ParseDecimal(string s) => decimal.TryParse(s, out var v) ? v : null;

        var divisionCache = new Dictionary<string, Division>(StringComparer.OrdinalIgnoreCase);
        var categoryCache = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        var subCategoryCache = new Dictionary<string, SubCategory>(StringComparer.OrdinalIgnoreCase);
        var productTypeCache = new Dictionary<string, ProductType>(StringComparer.OrdinalIgnoreCase);
        var packingTypeCache = new Dictionary<string, PackingType>(StringComparer.OrdinalIgnoreCase);
        var uomCache = new Dictionary<string, UomMaster>(StringComparer.OrdinalIgnoreCase);
        var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<(Product Product, decimal InitialStock)>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var divisionName = Cell(row, "Division");
            var categoryName = Cell(row, "Category");
            var subCategoryName = Cell(row, "SubCategory");
            var productTypeName = Cell(row, "ProductType");
            var name = Cell(row, "Name");

            if (string.IsNullOrWhiteSpace(divisionName) && string.IsNullOrWhiteSpace(name))
                continue; // fully blank row

            if (string.IsNullOrWhiteSpace(divisionName)) { Errors.Add($"Row {row}: Division is required."); continue; }
            if (string.IsNullOrWhiteSpace(categoryName)) { Errors.Add($"Row {row}: Category is required."); continue; }
            if (string.IsNullOrWhiteSpace(subCategoryName)) { Errors.Add($"Row {row}: SubCategory is required."); continue; }
            if (string.IsNullOrWhiteSpace(productTypeName)) { Errors.Add($"Row {row}: ProductType is required."); continue; }
            if (string.IsNullOrWhiteSpace(name)) { Errors.Add($"Row {row}: Name is required."); continue; }

            var division = await GetDivisionAsync(divisionName, divisionCache);
            if (division == null) { Errors.Add($"Row {row}: Division '{divisionName}' does not exist. Create it first under Masters > Divisions."); continue; }

            var category = await GetCategoryAsync(division, divisionName, categoryName, categoryCache);
            if (category == null) { Errors.Add($"Row {row}: Category '{categoryName}' does not exist in division '{divisionName}'. Create it first under Masters > Categories."); continue; }

            var subCategory = await GetSubCategoryAsync(category, $"{divisionName}||{categoryName}", subCategoryName, subCategoryCache);
            if (subCategory == null) { Errors.Add($"Row {row}: SubCategory '{subCategoryName}' does not exist in category '{categoryName}'. Create it first under Masters > Sub-Categories."); continue; }

            var productType = await GetProductTypeAsync(productTypeName, productTypeCache);
            if (productType == null) { Errors.Add($"Row {row}: ProductType '{productTypeName}' does not exist. Create it first under Masters > Product Types."); continue; }

            var packingTypeName = Cell(row, "PackingType");
            PackingType? packingType = null;
            if (!string.IsNullOrWhiteSpace(packingTypeName))
            {
                packingType = await GetPackingTypeAsync(packingTypeName, packingTypeCache);
                if (packingType == null) { Errors.Add($"Row {row}: PackingType '{packingTypeName}' does not exist. Create it first under Masters > Packing Types."); continue; }
            }

            var uomName = Cell(row, "UOM");
            UomMaster? uom = null;
            if (!string.IsNullOrWhiteSpace(uomName))
            {
                uom = await GetUomAsync(uomName, uomCache);
                if (uom == null) { Errors.Add($"Row {row}: UOM '{uomName}' does not exist. Create it first under Masters > UOM."); continue; }
            }

            string? productCode = NullIfEmpty(Cell(row, "ProductCode"));
            if (productCode != null)
            {
                productCode = productCode.ToUpperInvariant();
                if (!codesInFile.Add(productCode) || await _db.Products.AnyAsync(p => p.ProductCode == productCode))
                {
                    Errors.Add($"Row {row}: SKU '{productCode}' is already in use.");
                    continue;
                }
            }

            var initialStock = ParseDecimal(Cell(row, "InitialStock")) ?? 0;
            if (initialStock < 0) { Errors.Add($"Row {row}: Initial Stock must be 0 or greater."); continue; }

            var product = new Product
            {
                Division = division,
                Category = category,
                SubCategory = subCategory,
                ProductType = productType,
                PackingType = packingType,
                UomMaster = uom,
                ProductCode = productCode,
                Name = name,
                MarketName = NullIfEmpty(Cell(row, "MarketName")),
                Description = NullIfEmpty(Cell(row, "Description")),
                Rate = ParseDecimal(Cell(row, "Rate")),
                RatePer = NullIfEmpty(Cell(row, "RatePer")),
                Cut = ParseDecimal(Cell(row, "Cut")),
                QtyPerUnit = ParseDecimal(Cell(row, "QtyPerUnit")),
                Grade = NullIfEmpty(Cell(row, "Grade")),
                FabricComposition = NullIfEmpty(Cell(row, "FabricComposition")),
                Width = NullIfEmpty(Cell(row, "Width")),
                Weight = NullIfEmpty(Cell(row, "Weight")),
                Color = NullIfEmpty(Cell(row, "Color")),
                DesignNo = NullIfEmpty(Cell(row, "DesignNo")),
                Design = NullIfEmpty(Cell(row, "Design")),
                Brand = NullIfEmpty(Cell(row, "Brand"))
            };

            _db.Products.Add(product);
            pending.Add((product, initialStock));
        }

        if (pending.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No product rows found in the uploaded file.");
            return Page();
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Errors.Insert(0, "Import failed — no products were saved. " +
                "This usually means two rows share the same SKU/name combination, or a reference name conflicts with existing data. " +
                $"Details: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }

        foreach (var (product, initialStock) in pending)
        {
            if (string.IsNullOrEmpty(product.ProductCode))
            {
                var divisionCode = product.Division.Code;
                var prefix = string.IsNullOrWhiteSpace(divisionCode) ? "SD" : divisionCode.Trim().ToUpperInvariant();
                product.ProductCode = $"{prefix}{product.Id:D5}";
            }

            _db.InventoryRecords.Add(new InventoryRecord
            {
                ProductId = product.Id,
                QuantityAvailable = initialStock,
                UnitOfMeasure = product.UomMaster?.Name ?? "Units",
                LastSyncedAt = DateTime.UtcNow,
                SyncSource = initialStock > 0 ? "Import" : "System"
            });

            if (initialStock > 0)
            {
                _db.InventoryAdjustmentLogs.Add(new InventoryAdjustmentLog
                {
                    ProductId = product.Id,
                    AdjustmentType = "Set",
                    Quantity = initialStock,
                    PreviousQuantity = 0,
                    NewQuantity = initialStock,
                    Reason = "Initial stock via Excel import",
                    AdjustedBy = User.Identity?.Name ?? "Admin",
                    AdjustedAt = DateTime.UtcNow
                });
            }
        }
        await _db.SaveChangesAsync();

        ImportedCount = pending.Count;
        TempData["Success"] = $"Imported {ImportedCount} product(s).";
        return Page();
    }

    private async Task<Division?> GetDivisionAsync(string name, Dictionary<string, Division> cache)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        var existing = await _db.Divisions.FirstOrDefaultAsync(x => x.Name == name);
        if (existing != null) cache[name] = existing;
        return existing;
    }

    private async Task<Category?> GetCategoryAsync(Division division, string divisionKey, string name, Dictionary<string, Category> cache)
    {
        var key = $"{divisionKey}||{name}";
        if (cache.TryGetValue(key, out var cached)) return cached;
        var existing = await _db.Categories.FirstOrDefaultAsync(x => x.DivisionId == division.Id && x.Name == name);
        if (existing != null) cache[key] = existing;
        return existing;
    }

    private async Task<SubCategory?> GetSubCategoryAsync(Category category, string categoryKey, string name, Dictionary<string, SubCategory> cache)
    {
        var key = $"{categoryKey}||{name}";
        if (cache.TryGetValue(key, out var cached)) return cached;
        var existing = await _db.SubCategories.FirstOrDefaultAsync(x => x.CategoryId == category.Id && x.Name == name);
        if (existing != null) cache[key] = existing;
        return existing;
    }

    private async Task<ProductType?> GetProductTypeAsync(string name, Dictionary<string, ProductType> cache)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        var existing = await _db.ProductTypes.FirstOrDefaultAsync(x => x.Name == name);
        if (existing != null) cache[name] = existing;
        return existing;
    }

    private async Task<PackingType?> GetPackingTypeAsync(string name, Dictionary<string, PackingType> cache)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        var existing = await _db.PackingTypes.FirstOrDefaultAsync(x => x.Name == name);
        if (existing != null) cache[name] = existing;
        return existing;
    }

    private async Task<UomMaster?> GetUomAsync(string name, Dictionary<string, UomMaster> cache)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        var existing = await _db.UomMasters.FirstOrDefaultAsync(x => x.Name == name);
        if (existing != null) cache[name] = existing;
        return existing;
    }
}
