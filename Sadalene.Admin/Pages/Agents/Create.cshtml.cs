using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Agents;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        public string? AgentCode { get; set; }

        public List<CustomerRow> Customers { get; set; } = [];
    }

    public class CustomerRow
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GstNumber { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Drop rows the user left completely blank instead of forcing them to delete empty rows manually.
        var customerRows = Input.Customers
            .Where(c => !string.IsNullOrWhiteSpace(c.FullName) || !string.IsNullOrWhiteSpace(c.Phone))
            .ToList();
        Input.Customers = customerRows;

        for (int i = 0; i < customerRows.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(customerRows[i].FullName))
                ModelState.AddModelError($"Input.Customers[{i}].FullName", $"Row {i + 1}: customer name is required.");
            if (string.IsNullOrWhiteSpace(customerRows[i].Phone))
                ModelState.AddModelError($"Input.Customers[{i}].Phone", $"Row {i + 1}: customer phone is required.");
        }
        // Phone must be globally unique across Agents/Walk-in-Customers/Users — those are the identities
        // that log into the mobile app directly. An agent's own customers are exempt (ignored here).
        if (await _db.Agents.AnyAsync(x => x.Phone == Input.Phone))
            ModelState.AddModelError(string.Empty, "An agent with this phone number already exists.");
        else if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Phone == Input.Phone))
            ModelState.AddModelError(string.Empty, "A customer with this phone number already exists.");
        else if (await _db.Users.AnyAsync(x => x.Phone == Input.Phone))
            ModelState.AddModelError(string.Empty, "A staff user with this phone number already exists.");

        if (!string.IsNullOrWhiteSpace(Input.Email))
        {
            if (await _db.Agents.AnyAsync(x => x.Email == Input.Email))
                ModelState.AddModelError(string.Empty, "An agent with this email already exists.");
            else if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Email == Input.Email))
                ModelState.AddModelError(string.Empty, "A customer with this email already exists.");
        }
        if (!ModelState.IsValid) return Page();

        var agent = new Agent { FullName = Input.FullName, Phone = Input.Phone, Email = Input.Email, AgentCode = Input.AgentCode };
        _db.Agents.Add(agent);

        foreach (var c in customerRows)
        {
            _db.Customers.Add(new Customer
            {
                FullName = c.FullName!, Phone = c.Phone!, Email = c.Email,
                Address = c.Address, City = c.City, State = c.State, GstNumber = c.GstNumber,
                Agent = agent
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = customerRows.Count > 0
            ? $"Agent created with {customerRows.Count} customer(s)."
            : "Agent created.";
        return RedirectToPage("Index");
    }
}
