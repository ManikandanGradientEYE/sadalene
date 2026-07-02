using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

    public SelectList Categories { get; set; } = null!;
    public SelectList SubCategories { get; set; } = null!;
    public SelectList ProductTypes { get; set; } = null!;
    public SelectList Divisions { get; set; } = null!;

    public class InputModel
    {
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
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? ImageFile)
    {
        await LoadDropdownsAsync();
        if (!ModelState.IsValid) return Page();

        var product = new Product
        {
            Name = Input.Name, ProductCode = Input.ProductCode, Description = Input.Description,
            CategoryId = Input.CategoryId, SubCategoryId = Input.SubCategoryId,
            ProductTypeId = Input.ProductTypeId, DivisionId = Input.DivisionId,
            Brand = Input.Brand, Color = Input.Color, FabricComposition = Input.FabricComposition,
            Width = Input.Width, Weight = Input.Weight, Design = Input.Design,
            DisplayOrder = Input.DisplayOrder
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
        TempData["Success"] = "Product created.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        Categories   = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        SubCategories = new SelectList(await _db.SubCategories.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
        ProductTypes = new SelectList(await _db.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(), "Id", "Name");
        Divisions    = new SelectList(await _db.Divisions.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
    }
}
