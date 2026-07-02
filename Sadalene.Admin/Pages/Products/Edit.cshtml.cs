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

    public SelectList Categories { get; set; } = null!;
    public SelectList SubCategories { get; set; } = null!;
    public SelectList ProductTypes { get; set; } = null!;
    public SelectList Divisions { get; set; } = null!;

    public class InputModel
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? Description { get; set; }
        [Required] public int CategoryId { get; set; }
        [Required] public int SubCategoryId { get; set; }
        [Required] public int ProductTypeId { get; set; }
        [Required] public int DivisionId { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public string? FabricComposition { get; set; }
        public string? Width { get; set; }
        public string? Weight { get; set; }
        public string? Design { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();

        Input = new InputModel
        {
            Id = p.Id, Name = p.Name, ProductCode = p.ProductCode, Description = p.Description,
            CategoryId = p.CategoryId, SubCategoryId = p.SubCategoryId, ProductTypeId = p.ProductTypeId,
            DivisionId = p.DivisionId, Brand = p.Brand, Color = p.Color,
            FabricComposition = p.FabricComposition, Width = p.Width, Weight = p.Weight,
            Design = p.Design, DisplayOrder = p.DisplayOrder, IsActive = p.IsActive
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

        p.Name = Input.Name; p.ProductCode = Input.ProductCode; p.Description = Input.Description;
        p.CategoryId = Input.CategoryId; p.SubCategoryId = Input.SubCategoryId;
        p.ProductTypeId = Input.ProductTypeId; p.DivisionId = Input.DivisionId;
        p.Brand = Input.Brand; p.Color = Input.Color; p.FabricComposition = Input.FabricComposition;
        p.Width = Input.Width; p.Weight = Input.Weight; p.Design = Input.Design;
        p.DisplayOrder = Input.DisplayOrder; p.IsActive = Input.IsActive;
        p.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        Categories    = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        SubCategories = new SelectList(await _db.SubCategories.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
        ProductTypes  = new SelectList(await _db.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(), "Id", "Name");
        Divisions     = new SelectList(await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
    }
}
