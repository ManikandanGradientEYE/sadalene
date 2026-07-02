using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Products;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Divisions { get; set; } = null!;
    public SelectList ProductTypes { get; set; } = null!;
    public SelectList PackingTypes { get; set; } = null!;
    public SelectList UomList { get; set; } = null!;

    public class InputModel
    {
        public int Id { get; set; }
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
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();

        Input = new InputModel
        {
            Id                = p.Id,
            DivisionId        = p.DivisionId,
            CategoryId        = p.CategoryId,
            SubCategoryId     = p.SubCategoryId,
            ProductTypeId     = p.ProductTypeId,
            ProductCode       = p.ProductCode,
            Name              = p.Name,
            MarketName        = p.MarketName,
            Description       = p.Description,
            UomMasterId       = p.UomMasterId,
            Rate              = p.Rate,
            RatePer           = p.RatePer,
            Cut               = p.Cut,
            QtyPerUnit        = p.QtyPerUnit,
            PackingTypeId     = p.PackingTypeId,
            Grade             = p.Grade,
            FabricComposition = p.FabricComposition,
            Width             = p.Width,
            Weight            = p.Weight,
            Color             = p.Color,
            DesignNo          = p.DesignNo,
            Design            = p.Design,
            Brand             = p.Brand,
            DisplayOrder      = p.DisplayOrder,
            IsActive          = p.IsActive
        };
        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();
        if (!ModelState.IsValid) return Page();

        var p = await _db.Products.FindAsync(Input.Id);
        if (p == null) return NotFound();

        // Validate custom SKU uniqueness (excluding this product)
        var customSku = Input.ProductCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(customSku) && customSku != p.ProductCode &&
            await _db.Products.AnyAsync(x => x.ProductCode == customSku && x.Id != Input.Id))
        {
            ModelState.AddModelError("Input.ProductCode", $"SKU '{customSku}' is already in use by another product.");
            return Page();
        }

        p.DivisionId        = Input.DivisionId;
        p.CategoryId        = Input.CategoryId;
        p.SubCategoryId     = Input.SubCategoryId;
        p.ProductTypeId     = Input.ProductTypeId;
        p.ProductCode       = string.IsNullOrEmpty(customSku) ? p.ProductCode : customSku;
        p.Name              = Input.Name;
        p.MarketName        = Input.MarketName;
        p.Description       = Input.Description;
        p.UomMasterId       = Input.UomMasterId == 0 ? null : Input.UomMasterId;
        p.Rate              = Input.Rate;
        p.RatePer           = Input.RatePer;
        p.Cut               = Input.Cut;
        p.QtyPerUnit        = Input.QtyPerUnit;
        p.PackingTypeId     = Input.PackingTypeId == 0 ? null : Input.PackingTypeId;
        p.Grade             = Input.Grade;
        p.FabricComposition = Input.FabricComposition;
        p.Width             = Input.Width;
        p.Weight            = Input.Weight;
        p.Color             = Input.Color;
        p.DesignNo          = Input.DesignNo;
        p.Design            = Input.Design;
        p.Brand             = Input.Brand;
        p.DisplayOrder      = Input.DisplayOrder;
        p.IsActive          = Input.IsActive;
        p.UpdatedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
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
