using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.Categories;

[RequestSizeLimit(20_000_000)]
public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ImportModel(ApplicationDbContext db) => _db = db;

    public List<string> Errors { get; set; } = [];
    public int ImportedCount { get; set; }

    private static readonly string[] RequiredHeaders = ["Division", "Name"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("Division", true), ("Name", true), ("Description", false), ("DisplayOrder", false)
    ];

    public void OnGet() { }

    public async Task<IActionResult> OnGetTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Categories");

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
        ws.Cell(2, 3).Value = "Silk sarees";
        ws.Cell(2, 4).Value = 0;
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        var refWs = wb.Worksheets.Add("Reference (existing divisions)");
        refWs.Cell(1, 1).Value = "Divisions";
        refWs.Cell(1, 1).Style.Font.Bold = true;
        var divisions = await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name).Select(d => d.Name).ToListAsync();
        for (int i = 0; i < divisions.Count; i++) refWs.Cell(i + 2, 1).Value = divisions[i];
        refWs.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Category-Import-Template.xlsx");
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

        var divisionCache = new Dictionary<string, Division>(StringComparer.OrdinalIgnoreCase);
        var namesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<Category>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var divisionName = Cell(row, "Division");
            var name = Cell(row, "Name");

            if (string.IsNullOrWhiteSpace(divisionName) && string.IsNullOrWhiteSpace(name))
                continue; // fully blank row

            if (string.IsNullOrWhiteSpace(divisionName)) { Errors.Add($"Row {row}: Division is required."); continue; }
            if (string.IsNullOrWhiteSpace(name)) { Errors.Add($"Row {row}: Name is required."); continue; }

            if (!divisionCache.TryGetValue(divisionName, out var division))
            {
                division = await _db.Divisions.FirstOrDefaultAsync(d => d.Name == divisionName);
                if (division == null)
                {
                    Errors.Add($"Row {row}: Division '{divisionName}' was not found. Import Divisions first or check the spelling.");
                    continue;
                }
                divisionCache[divisionName] = division;
            }

            var fileKey = $"{division.Id}||{name}";
            if (!namesInFile.Add(fileKey) || await _db.Categories.AnyAsync(c => c.DivisionId == division.Id && c.Name == name))
            {
                Errors.Add($"Row {row}: Category '{name}' already exists in division '{divisionName}'.");
                continue;
            }

            pending.Add(new Category
            {
                Division     = division,
                Name         = name,
                Description  = Cell(row, "Description"),
                DisplayOrder = ParseInt(Cell(row, "DisplayOrder"))
            });
        }

        if (pending.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No category rows found in the uploaded file.");
            return Page();
        }

        _db.Categories.AddRange(pending);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Errors.Insert(0, "Import failed — no categories were saved. " +
                "This usually means two rows share the same name within a division. " +
                $"Details: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }

        ImportedCount = pending.Count;
        TempData["Success"] = $"Imported {ImportedCount} category(ies).";
        return Page();
    }
}
