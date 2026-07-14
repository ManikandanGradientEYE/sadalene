using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Controllers;

[ApiController]
[Route("api/master")]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public MasterDataController(ApplicationDbContext db) => _db = db;

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(int divisionId)
    {
        var items = await _db.Categories
            .Where(c => c.IsActive && c.DivisionId == divisionId)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("subcategories")]
    public async Task<IActionResult> GetSubCategories(int categoryId)
    {
        var items = await _db.SubCategories
            .Where(s => s.IsActive && s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.UomMasterId })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("agent-customers")]
    public async Task<IActionResult> GetAgentCustomers(int agentId)
    {
        var items = await _db.Customers
            .Where(c => c.IsActive && c.AgentId == agentId)
            .OrderBy(c => c.FullName)
            .Select(c => new { c.Id, c.FullName, c.Phone })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("product-info")]
    public async Task<IActionResult> GetProductInfo(int productId)
    {
        var product = await _db.Products
            .Include(p => p.UomMaster)
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound();

        var stock = await _db.InventoryRecords
            .Where(i => i.ProductId == productId)
            .SumAsync(i => (decimal?)i.QuantityAvailable) ?? 0;

        return Ok(new
        {
            uom = product.UomMaster?.Name ?? "Units",
            stock,
            allowsHalfUnit = product.UomMaster?.AllowsHalfUnit ?? false,
            rate = product.Rate ?? 0
        });
    }

    [HttpGet("product-by-sku")]
    public async Task<IActionResult> GetProductBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku)) return NotFound();
        var term = sku.Trim();

        var product = await _db.Products
            .Include(p => p.UomMaster)
            .FirstOrDefaultAsync(p => p.IsActive && p.ProductCode == term);
        if (product == null) return NotFound();

        var stock = await _db.InventoryRecords
            .Where(i => i.ProductId == product.Id)
            .SumAsync(i => (decimal?)i.QuantityAvailable) ?? 0;

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            code = product.ProductCode,
            uom = product.UomMaster?.Name ?? "Units",
            stock,
            allowsHalfUnit = product.UomMaster?.AllowsHalfUnit ?? false,
            rate = product.Rate ?? 0
        });
    }
}
