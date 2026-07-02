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
}
