using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Admin.Services;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

public class BarcodeModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly BarcodeService _barcodeService;
    public BarcodeModel(ApplicationDbContext db, BarcodeService barcodeService)
    {
        _db = db;
        _barcodeService = barcodeService;
    }

    public Product? Product { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Division)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (Product == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateAsync(int id, string barcodeValue)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        var imageUrl = _barcodeService.GenerateQrCode(barcodeValue, $"product-{id}");

        product.BarcodeValue      = barcodeValue;
        product.BarcodeImageUrl   = imageUrl;
        product.IsBarcodeActive   = true;
        product.BarcodeGeneratedAt = DateTime.UtcNow;
        product.UpdatedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Barcode generated successfully.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsBarcodeActive = false;
        product.UpdatedAt       = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Barcode deactivated.";
        return RedirectToPage(new { id });
    }
}
