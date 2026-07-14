using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.SubCategories;

[RequestSizeLimit(20_000_000)]
public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ImportModel(ApplicationDbContext db) => _db = db;

    public List<string> Errors { get; set; } = [];
    public int ImportedCount { get; set; }

    private static readonly string[] RequiredHeaders = ["Division", "Category", "Name"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("Division", true), ("Category", true), ("Name", true), ("UOM", false), ("Description", false), ("DisplayOrder", false)
    ];

    public void OnGet() { }

    public async Task<IActionResult> OnGetTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("SubCategories");

        for (int i = 0; i < TemplateColumns.Length; i++)
        {
            var (header, required) = TemplateColumns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = required ? $"{header}*" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.FromHtml("#fde8e8") : XLColor.FromHtml("#eef2f7");
        }
        ws.Cell(2, 1).Value = "Silk";
        ws.Cell(2, 2).Value = "Saree";
        ws.Cell(2, 3).Value = "Kanjivaram";
        ws.Cell(2, 4).Value = "Taka";
        ws.Cell(2, 5).Value = "Kanjivaram silk sarees";
        ws.Cell(2, 6).Value = 0;
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        var refWs = wb.Worksheets.Add("Reference (existing names)");
        var refHeaders = new[] { "Categories (Division / Category)", "UOMs" };
        for (int i = 0; i < refHeaders.Length; i++)
        {
            refWs.Cell(1, i + 1).Value = refHeaders[i];
            refWs.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var categories = await _db.Categories.Where(c => c.IsActive).Include(c => c.Division)
            .OrderBy(c => c.Division.Name).ThenBy(c => c.Name)
            .Select(c => c.Division.Name + " / " + c.Name).ToListAsync();
        var uoms = await _db.UomMasters.Where(u => u.IsActive).OrderBy(u => u.Name).Select(u => u.Name).ToListAsync();

        var lists = new[] { categories, uoms };
        for (int col = 0; col < lists.Length; col++)
            for (int i = 0; i < lists[col].Count; i++)
                refWs.Cell(i + 2, col + 1).Value = lists[col][i];
        refWs.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "SubCategory-Import-Template.xlsx");
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
        static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

        var categoryCache = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        var uomCache = new Dictionary<string, UomMaster?>(StringComparer.OrdinalIgnoreCase);
        var namesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<SubCategory>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var divisionName = Cell(row, "Division");
            var categoryName = Cell(row, "Category");
            var name = Cell(row, "Name");

            if (string.IsNullOrWhiteSpace(divisionName) && string.IsNullOrWhiteSpace(name))
                continue; // fully blank row

            if (string.IsNullOrWhiteSpace(divisionName)) { Errors.Add($"Row {row}: Division is required."); continue; }
            if (string.IsNullOrWhiteSpace(categoryName)) { Errors.Add($"Row {row}: Category is required."); continue; }
            if (string.IsNullOrWhiteSpace(name)) { Errors.Add($"Row {row}: Name is required."); continue; }

            var categoryKey = $"{divisionName}||{categoryName}";
            if (!categoryCache.TryGetValue(categoryKey, out var category))
            {
                category = await _db.Categories.Include(c => c.Division)
                    .FirstOrDefaultAsync(c => c.Division.Name == divisionName && c.Name == categoryName);
                if (category == null)
                {
                    Errors.Add($"Row {row}: Category '{categoryName}' was not found in division '{divisionName}'. Import Categories first or check the spelling.");
                    continue;
                }
                categoryCache[categoryKey] = category;
            }

            var uomName = Cell(row, "UOM");
            UomMaster? uom = null;
            if (!string.IsNullOrWhiteSpace(uomName))
            {
                if (!uomCache.TryGetValue(uomName, out uom))
                {
                    uom = await _db.UomMasters.FirstOrDefaultAsync(u => u.Name == uomName);
                    if (uom == null)
                    {
                        Errors.Add($"Row {row}: UOM '{uomName}' was not found. Import UOMs first or check the spelling.");
                        continue;
                    }
                    uomCache[uomName] = uom;
                }
            }

            var fileKey = $"{category.Id}||{name}";
            if (!namesInFile.Add(fileKey) || await _db.SubCategories.AnyAsync(s => s.CategoryId == category.Id && s.Name == name))
            {
                Errors.Add($"Row {row}: Sub-category '{name}' already exists in category '{categoryName}'.");
                continue;
            }

            pending.Add(new SubCategory
            {
                Category     = category,
                Name         = name,
                UomMaster    = uom,
                Description  = Cell(row, "Description"),
                DisplayOrder = ParseInt(Cell(row, "DisplayOrder"))
            });
        }

        if (pending.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No sub-category rows found in the uploaded file.");
            return Page();
        }

        _db.SubCategories.AddRange(pending);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Errors.Insert(0, "Import failed — no sub-categories were saved. " +
                "This usually means two rows share the same name within a category. " +
                $"Details: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }

        ImportedCount = pending.Count;
        TempData["Success"] = $"Imported {ImportedCount} sub-category(ies).";
        return Page();
    }
}
