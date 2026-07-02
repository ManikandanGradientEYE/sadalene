using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Divisions { get; set; } = null!;
    public SelectList ProductTypes { get; set; } = null!;
    public SelectList PackingTypes { get; set; } = null!;
    public SelectList UomList { get; set; } = null!;

    public class InputModel
    {
        [Required] public int DivisionId { get; set; }
        [Required] public int CategoryId { get; set; }
        [Required] public int SubCategoryId { get; set; }
        [Required] public int ProductTypeId { get; set; }

        public string? ProductCode { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? MarketName { get; set; }
        public string? Description { get; set; }

        // Pricing & UOM
        public int? UomMasterId { get; set; }
        public decimal? Rate { get; set; }
        public string? RatePer { get; set; }
        public decimal? Cut { get; set; }
        public decimal? QtyPerUnit { get; set; }
        public int? PackingTypeId { get; set; }
        public string? Grade { get; set; }

        // Specifications
        public string? FabricComposition { get; set; }
        public string? Width { get; set; }
        public string? Weight { get; set; }
        public string? Color { get; set; }
        public string? DesignNo { get; set; }
        public string? Design { get; set; }
        public string? Brand { get; set; }

        public int DisplayOrder { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? ImageFile)
    {
        await LoadDropdownsAsync();
        if (!ModelState.IsValid) return Page();

        // Validate custom SKU uniqueness before saving
        var customSku = Input.ProductCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(customSku) &&
            await _db.Products.AnyAsync(p => p.ProductCode == customSku))
        {
            ModelState.AddModelError("Input.ProductCode", $"SKU '{customSku}' is already in use.");
            return Page();
        }

        var product = new Product
        {
            DivisionId        = Input.DivisionId,
            CategoryId        = Input.CategoryId,
            SubCategoryId     = Input.SubCategoryId,
            ProductTypeId     = Input.ProductTypeId,
            ProductCode       = customSku,          // null if blank — auto-generated below
            Name              = Input.Name,
            MarketName        = Input.MarketName,
            Description       = Input.Description,
            UomMasterId       = Input.UomMasterId == 0 ? null : Input.UomMasterId,
            Rate              = Input.Rate,
            RatePer           = Input.RatePer,
            Cut               = Input.Cut,
            QtyPerUnit        = Input.QtyPerUnit,
            PackingTypeId     = Input.PackingTypeId == 0 ? null : Input.PackingTypeId,
            Grade             = Input.Grade,
            FabricComposition = Input.FabricComposition,
            Width             = Input.Width,
            Weight            = Input.Weight,
            Color             = Input.Color,
            DesignNo          = Input.DesignNo,
            Design            = Input.Design,
            Brand             = Input.Brand,
            DisplayOrder      = Input.DisplayOrder
        };

        if (ImageFile?.Length > 0)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await ImageFile.CopyToAsync(stream);
            product.Images.Add(new ProductImage { ImageUrl = $"/uploads/products/{fileName}", IsPrimary = true });
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Auto-generate SKU from assigned PK if staff left it blank
        if (string.IsNullOrEmpty(product.ProductCode))
        {
            product.ProductCode = $"SD{product.Id:D5}";
        }

        // Bootstrap inventory record at zero stock
        var uomName = await _db.UomMasters
            .Where(u => u.Id == product.UomMasterId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync() ?? "Units";

        _db.InventoryRecords.Add(new InventoryRecord
        {
            ProductId         = product.Id,
            QuantityAvailable = 0,
            UnitOfMeasure     = uomName,
            LastSyncedAt      = DateTime.UtcNow,
            SyncSource        = "System"
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Product '{Input.Name}' created with SKU {product.ProductCode}.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        Divisions    = new SelectList(await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
        ProductTypes = new SelectList(await _db.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(), "Id", "Name");
        PackingTypes = new SelectList(await _db.PackingTypes.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
        var uoms = await _db.UomMasters.Where(u => u.IsActive).OrderBy(u => u.Name)
            .Select(u => new { u.Id, Label = u.Abbreviation != null ? $"{u.Name} ({u.Abbreviation})" : u.Name })
            .ToListAsync();
        UomList = new SelectList(uoms, "Id", "Label");
    }
}
