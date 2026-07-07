using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
