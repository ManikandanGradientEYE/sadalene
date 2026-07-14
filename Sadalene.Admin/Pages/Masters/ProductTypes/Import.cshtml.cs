using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Masters;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Masters.ProductTypes;

[RequestSizeLimit(20_000_000)]
public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ImportModel(ApplicationDbContext db) => _db = db;

    public List<string> Errors { get; set; } = [];
    public int ImportedCount { get; set; }

    private static readonly string[] RequiredHeaders = ["Name"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("Name", true), ("Description", false)
    ];

    public void OnGet() { }

    public async Task<IActionResult> OnGetTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("ProductTypes");

        for (int i = 0; i < TemplateColumns.Length; i++)
        {
            var (header, required) = TemplateColumns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = required ? $"{header}*" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.FromHtml("#fde8e8") : XLColor.FromHtml("#eef2f7");
        }
        ws.Cell(2, 1).Value = "Saree";
        ws.Cell(2, 2).Value = "Traditional draped garment";
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ProductType-Import-Template.xlsx");
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

        var namesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<ProductType>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var name = Cell(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
                continue; // fully blank row

            var description = Cell(row, "Description");

            if (!namesInFile.Add(name) || await _db.ProductTypes.AnyAsync(t => t.Name == name))
            {
                Errors.Add($"Row {row}: Product type '{name}' already exists.");
                continue;
            }

            pending.Add(new ProductType { Name = name, Description = description });
        }

        if (pending.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No product type rows found in the uploaded file.");
            return Page();
        }

        _db.ProductTypes.AddRange(pending);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Errors.Insert(0, "Import failed — no product types were saved. " +
                "This usually means two rows share the same name. " +
                $"Details: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }

        ImportedCount = pending.Count;
        TempData["Success"] = $"Imported {ImportedCount} product type(s).";
        return Page();
    }
}
