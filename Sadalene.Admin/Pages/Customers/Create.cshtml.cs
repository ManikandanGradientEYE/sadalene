using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Agents { get; set; } = null!;

    public class InputModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        [Required] public string Phone { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GstNumber { get; set; }
        public int? AgentId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadAgentsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAgentsAsync();

        if (!ModelState.IsValid) return Page();

        var agentId = Input.AgentId is null or 0 ? null : Input.AgentId;
        var customerCode = string.IsNullOrWhiteSpace(Input.CustomerCode) ? null : Input.CustomerCode.Trim().ToUpperInvariant();

        if (customerCode != null && await _db.Customers.AnyAsync(x => x.CustomerCode == customerCode))
        {
            ModelState.AddModelError(string.Empty, $"A customer with code '{customerCode}' already exists.");
            return Page();
        }

        // Phone must be globally unique across Agents/Walk-in-Customers/Users — those are the identities
        // that log into the mobile app directly. An agent's own customers are exempt: they never log
        // in themselves (the agent orders on their behalf), so their numbers don't need to be unique.
        if (agentId == null)
        {
            if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Phone == Input.Phone))
            {
                ModelState.AddModelError(string.Empty, "A customer with this phone number already exists.");
                return Page();
            }
            if (await _db.Agents.AnyAsync(x => x.Phone == Input.Phone))
            {
                ModelState.AddModelError(string.Empty, "An agent with this phone number already exists.");
                return Page();
            }
            if (await _db.Users.AnyAsync(x => x.Phone == Input.Phone))
            {
                ModelState.AddModelError(string.Empty, "A staff user with this phone number already exists.");
                return Page();
            }
        }
        if (!string.IsNullOrWhiteSpace(Input.Email))
        {
            if (await _db.Customers.AnyAsync(x => x.AgentId == agentId && x.Email == Input.Email))
            {
                ModelState.AddModelError(string.Empty, "A customer with this email already exists.");
                return Page();
            }
            if (await _db.Agents.AnyAsync(x => x.Email == Input.Email))
            {
                ModelState.AddModelError(string.Empty, "An agent with this email already exists.");
                return Page();
            }
        }

        _db.Customers.Add(new Customer
        {
            FullName  = Input.FullName, CustomerCode = customerCode, Phone = Input.Phone, Email = Input.Email,
            Address   = Input.Address,  City  = Input.City,  State = Input.State,
            GstNumber = Input.GstNumber,
            AgentId   = agentId
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadAgentsAsync()
    {
        Agents = new SelectList(await _db.Agents.Where(a => a.IsActive).OrderBy(a => a.FullName).ToListAsync(), "Id", "FullName");
    }
}
