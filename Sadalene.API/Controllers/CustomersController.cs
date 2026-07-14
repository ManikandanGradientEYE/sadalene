using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.DTOs.Customers;
using Sadalene.API.Extensions;
using Sadalene.Infrastructure.Data;

namespace Sadalene.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CustomersController(ApplicationDbContext db) => _db = db;

    /// <summary>An Agent's own customers — used to populate "who am I acting for" pickers.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> GetMine()
    {
        var agentId = User.GetIdentityId();
        var items = await _db.Customers
            .Where(c => c.IsActive && c.AgentId == agentId)
            .OrderBy(c => c.FullName)
            .Select(c => new CustomerListItemDto(c.Id, c.FullName, c.Phone, c.City))
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>Staff-only lookup across all customers (walk-in search).</summary>
    [HttpGet("search")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Search(string? q)
    {
        var query = _db.Customers.Where(c => c.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(c => c.FullName.Contains(term) || c.Phone.Contains(term));
        }

        var items = await query.OrderBy(c => c.FullName).Take(50)
            .Select(c => new CustomerListItemDto(c.Id, c.FullName, c.Phone, c.City))
            .ToListAsync();
        return Ok(items);
    }
}
