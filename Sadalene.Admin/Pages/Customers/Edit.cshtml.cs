using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Customers;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GstNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c == null) return NotFound();
        Input = new InputModel { Id = c.Id, FullName = c.FullName, Phone = c.Phone, Email = c.Email,
            Address = c.Address, City = c.City, State = c.State, GstNumber = c.GstNumber, IsActive = c.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var c = await _db.Customers.FindAsync(Input.Id);
        if (c == null) return NotFound();

        // Phone must be globally unique across Agents/Walk-in-Customers/Users — those are the identities
        // that log into the mobile app directly. An agent's own customers are exempt: they never log
        // in themselves (the agent orders on their behalf), so their numbers don't need to be unique.
        if (c.AgentId == null)
        {
            if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Phone == Input.Phone && x.Id != Input.Id))
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

        // Email uniqueness still only applies to walk-in customers (no agent) — matches the DB's filtered unique index.
        if (c.AgentId == null && !string.IsNullOrWhiteSpace(Input.Email))
        {
            if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Email == Input.Email && x.Id != Input.Id))
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

        c.FullName = Input.FullName; c.Phone = Input.Phone; c.Email = Input.Email;
        c.Address  = Input.Address;  c.City  = Input.City;  c.State = Input.State;
        c.GstNumber = Input.GstNumber; c.IsActive = Input.IsActive; c.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer updated.";
        return RedirectToPage("Index");
    }
}
