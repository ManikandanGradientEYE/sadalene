using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Documents;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Documents.Challans;

[RequestSizeLimit(100_000_000)]
public class ImportModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public ImportModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public List<string> Errors { get; set; } = [];
    public int ImportedCount { get; set; }

    private static readonly string[] RequiredHeaders = ["ChallanNumber", "Customer", "ChallanDate"];
    private static readonly (string Header, bool Required)[] TemplateColumns =
    [
        ("ChallanNumber", true), ("Customer", true), ("OrderNumber", false), ("ChallanDate", true), ("FileName", false)
    ];

    public void OnGet() { }

    public async Task<IActionResult> OnGetTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Challans");

        for (int i = 0; i < TemplateColumns.Length; i++)
        {
            var (header, required) = TemplateColumns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = required ? $"{header}*" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.FromHtml("#fde8e8") : XLColor.FromHtml("#eef2f7");
        }
        ws.Cell(2, 1).Value = "CH-0001";
        ws.Cell(2, 2).Value = "9876543210 or Customer Full Name";
        ws.Cell(2, 3).Value = "ORD-2026-000001";
        ws.Cell(2, 4).Value = DateTime.Today;
        ws.Cell(2, 4).Style.DateFormat.Format = "dd-MM-yyyy";
        ws.Cell(2, 5).Value = "CH-0001.pdf";
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();

        var refWs = wb.Worksheets.Add("Reference (existing records)");
        var refHeaders = new[] { "Customers (Name / Phone)", "Recent Orders (Order # / Customer)" };
        for (int i = 0; i < refHeaders.Length; i++)
        {
            refWs.Cell(1, i + 1).Value = refHeaders[i];
            refWs.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.FullName)
            .Select(c => c.FullName + " / " + c.Phone).ToListAsync();
        var orders = await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(200)
            .Select(o => o.OrderNumber + " / " + o.Customer.FullName).ToListAsync();

        var lists = new[] { customers, orders };
        for (int col = 0; col < lists.Length; col++)
            for (int i = 0; i < lists[col].Count; i++)
                refWs.Cell(i + 2, col + 1).Value = lists[col][i];
        refWs.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Challan-Import-Template.xlsx");
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file, IFormFile? zipFile)
    {
        if (file == null || file.Length == 0)
        {
            Errors.Add("Please choose an Excel file to import.");
            return Page();
        }

        Dictionary<string, byte[]>? zipEntries = null;
        if (zipFile is { Length: > 0 })
        {
            zipEntries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var zipStream = zipFile.OpenReadStream();
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue; // directory entry
                    using var entryStream = entry.Open();
                    using var ms = new MemoryStream();
                    await entryStream.CopyToAsync(ms);
                    zipEntries[entry.Name] = ms.ToArray();
                }
            }
            catch (InvalidDataException)
            {
                Errors.Add("Could not read the uploaded ZIP file. Please make sure it's a valid .zip.");
                return Page();
            }
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

        DateTime? ParseDate(int row, string header)
        {
            if (!colMap.TryGetValue(header, out var col)) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            return DateTime.TryParse(cell.GetString().Trim(), out var parsed) ? parsed : null;
        }

        var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<Challan>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var challanNumber = Cell(row, "ChallanNumber");
            var customerRef = Cell(row, "Customer");
            var orderNumber = NullIfEmpty(Cell(row, "OrderNumber"));

            if (string.IsNullOrWhiteSpace(challanNumber) && string.IsNullOrWhiteSpace(customerRef))
                continue; // fully blank row

            if (string.IsNullOrWhiteSpace(challanNumber)) { Errors.Add($"Row {row}: Challan Number is required."); continue; }
            if (string.IsNullOrWhiteSpace(customerRef)) { Errors.Add($"Row {row}: Customer is required."); continue; }

            if (!codesInFile.Add(challanNumber) || await _db.Challans.AnyAsync(c => c.ChallanNumber == challanNumber))
            {
                Errors.Add($"Row {row}: Challan Number '{challanNumber}' already exists.");
                continue;
            }

            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Phone == customerRef || c.FullName == customerRef);
            if (customer == null)
            {
                Errors.Add($"Row {row}: Customer '{customerRef}' not found (match by phone or full name).");
                continue;
            }

            int? orderId = null;
            if (orderNumber != null)
            {
                var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
                if (order == null)
                {
                    Errors.Add($"Row {row}: Order '{orderNumber}' not found.");
                    continue;
                }
                orderId = order.Id;
            }

            var challanDate = ParseDate(row, "ChallanDate");
            if (challanDate == null)
            {
                Errors.Add($"Row {row}: Challan Date '{Cell(row, "ChallanDate")}' is not a valid date.");
                continue;
            }

            var fileName = NullIfEmpty(Cell(row, "FileName"));
            string? fileUrl = null;
            if (fileName != null)
            {
                if (zipEntries == null)
                {
                    Errors.Add($"Row {row}: FileName '{fileName}' was specified but no ZIP file was uploaded.");
                    continue;
                }
                if (!zipEntries.TryGetValue(fileName, out var bytes))
                {
                    Errors.Add($"Row {row}: File '{fileName}' was not found in the uploaded ZIP.");
                    continue;
                }

                var folder = Path.Combine(_env.WebRootPath, "uploads", "challans");
                Directory.CreateDirectory(folder);
                var savedName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(folder, savedName), bytes);
                fileUrl = $"/uploads/challans/{savedName}";
            }

            pending.Add(new Challan
            {
                ChallanNumber = challanNumber,
                CustomerId    = customer.Id,
                OrderId       = orderId,
                ChallanDate   = challanDate.Value,
                FileUrl       = fileUrl
            });
        }

        if (pending.Count == 0)
        {
            if (Errors.Count == 0) Errors.Add("No challan rows found in the uploaded file.");
            return Page();
        }

        _db.Challans.AddRange(pending);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Errors.Insert(0, "Import failed — no challans were saved. " +
                "This usually means two rows share the same Challan Number, or a reference doesn't match existing data. " +
                $"Details: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }

        ImportedCount = pending.Count;
        TempData["Success"] = $"Imported {ImportedCount} challan(s).";
        return Page();
    }
}
