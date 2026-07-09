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
    public SelectList Categories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    public SelectList SubCategories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
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

        [Range(0, double.MaxValue, ErrorMessage = "Initial Stock must be 0 or greater.")]
        public decimal? InitialStock { get; set; }
    }

    public async Task OnGetAsync()
    {
        // Classification is usually the same across a run of products entered back-to-back,
        // so default it to the last-created product's values instead of making staff re-pick it every time.
        var last = await _db.Products
            .OrderByDescending(p => p.Id)
            .Select(p => new { p.DivisionId, p.CategoryId, p.SubCategoryId, p.ProductTypeId })
            .FirstOrDefaultAsync();

        if (last != null)
        {
            Input.DivisionId    = last.DivisionId;
            Input.CategoryId    = last.CategoryId;
            Input.SubCategoryId = last.SubCategoryId;
            Input.ProductTypeId = last.ProductTypeId;
        }

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

        // Auto-generate SKU from assigned PK if staff left it blank, prefixed with the Division's code
        if (string.IsNullOrEmpty(product.ProductCode))
        {
            var divisionCode = await _db.Divisions
                .Where(d => d.Id == product.DivisionId)
                .Select(d => d.Code)
                .FirstOrDefaultAsync();

            var prefix = string.IsNullOrWhiteSpace(divisionCode) ? "SD" : divisionCode.Trim().ToUpperInvariant();
            product.ProductCode = $"{prefix}{product.Id:D5}";
        }

        // Bootstrap inventory record with optional initial stock
        var uomName = await _db.UomMasters
            .Where(u => u.Id == product.UomMasterId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync() ?? "Units";

        var initialQty = Input.InitialStock ?? 0;

        _db.InventoryRecords.Add(new InventoryRecord
        {
            ProductId         = product.Id,
            QuantityAvailable = initialQty,
            UnitOfMeasure     = uomName,
            LastSyncedAt      = DateTime.UtcNow,
            SyncSource        = initialQty > 0 ? "Manual" : "System"
        });

        if (initialQty > 0)
        {
            _db.InventoryAdjustmentLogs.Add(new InventoryAdjustmentLog
            {
                ProductId        = product.Id,
                AdjustmentType   = "Set",
                Quantity         = initialQty,
                PreviousQuantity = 0,
                NewQuantity      = initialQty,
                Reason           = "Initial stock on product creation",
                AdjustedBy       = User.Identity?.Name ?? "Admin",
                AdjustedAt       = DateTime.UtcNow
            });
        }

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

        // Pre-populate the cascading Category/Sub-Category dropdowns to match whatever Division/Category
        // is already selected (from the last-record default above, or from a postback that failed validation).
        if (Input.DivisionId > 0)
        {
            Categories = new SelectList(
                await _db.Categories.Where(c => c.IsActive && c.DivisionId == Input.DivisionId).OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name");
        }

        if (Input.CategoryId > 0)
        {
            SubCategories = new SelectList(
                await _db.SubCategories.Where(s => s.IsActive && s.CategoryId == Input.CategoryId).OrderBy(s => s.Name).ToListAsync(),
                "Id", "Name");
        }
    }
}
