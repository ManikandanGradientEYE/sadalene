using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.DTOs.Catalog;
using Sadalene.Core.Entities.Products;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;

namespace Sadalene.API.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize]
public class CatalogController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CatalogController(ApplicationDbContext db) => _db = db;

    [HttpGet("divisions")]
    public async Task<IActionResult> GetDivisions()
    {
        var items = await _db.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DivisionDto(d.Id, d.Name, d.Code))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(int divisionId)
    {
        var items = await _db.Categories
            .Where(c => c.IsActive && c.DivisionId == divisionId)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("subcategories")]
    public async Task<IActionResult> GetSubCategories(int categoryId)
    {
        var items = await _db.SubCategories
            .Where(s => s.IsActive && s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .Select(s => new SubCategoryDto(s.Id, s.Name, s.UomMasterId))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        int? divisionId, int? categoryId, int? subCategoryId, string? search, int page = 1, int pageSize = 20)
    {
        var query = _db.Products.Where(p => p.IsActive).AsQueryable();

        if (divisionId.HasValue) query = query.Where(p => p.DivisionId == divisionId);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (subCategoryId.HasValue) query = query.Where(p => p.SubCategoryId == subCategoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || (p.ProductCode != null && p.ProductCode.Contains(term)));
        }

        var projected = query
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItemDto(
                p.Id,
                p.Name,
                p.ProductCode,
                p.MarketName,
                p.Rate,
                p.UomMaster != null ? p.UomMaster.Name : "Units",
                p.InventoryRecords.Sum(i => (decimal?)i.QuantityAvailable) ?? 0,
                p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                p.DivisionId,
                p.Division.Name
            ));

        var result = await projected.ToPagedResultAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.Division).Include(p => p.Category).Include(p => p.SubCategory)
            .Include(p => p.UomMaster).Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return NotFound();

        var stock = await _db.InventoryRecords.Where(i => i.ProductId == id).SumAsync(i => (decimal?)i.QuantityAvailable) ?? 0;
        return Ok(MapDetail(product, stock));
    }

    [HttpGet("products/by-sku")]
    public async Task<IActionResult> GetProductBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku)) return NotFound();
        var term = sku.Trim();

        var product = await _db.Products
            .Include(p => p.Division).Include(p => p.Category).Include(p => p.SubCategory)
            .Include(p => p.UomMaster).Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.IsActive && p.ProductCode == term);
        if (product == null) return NotFound();

        var stock = await _db.InventoryRecords.Where(i => i.ProductId == product.Id).SumAsync(i => (decimal?)i.QuantityAvailable) ?? 0;
        return Ok(MapDetail(product, stock));
    }

    private static ProductDetailDto MapDetail(Product product, decimal stock) => new(
        product.Id, product.Name, product.MarketName, product.Description, product.ProductCode,
        product.DivisionId, product.Division.Name,
        product.CategoryId, product.Category.Name,
        product.SubCategoryId, product.SubCategory.Name,
        product.Rate, product.RatePer,
        product.UomMaster?.Name ?? "Units",
        product.UomMaster?.AllowsHalfUnit ?? false,
        stock,
        product.Grade, product.FabricComposition, product.Width, product.Weight, product.Color,
        product.DesignNo, product.Design, product.Brand,
        product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()
    );
}
