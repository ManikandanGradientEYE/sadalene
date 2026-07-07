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

        public bool AddCustomer { get; set; }
        public string? CustomerFullName { get; set; }
        public string? CustomerPhone { get; set; }
        [EmailAddress] public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerGstNumber { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.AddCustomer)
        {
            if (string.IsNullOrWhiteSpace(Input.CustomerFullName))
                ModelState.AddModelError("Input.CustomerFullName", "Customer name is required.");
            if (string.IsNullOrWhiteSpace(Input.CustomerPhone))
                ModelState.AddModelError("Input.CustomerPhone", "Customer phone is required.");
        }
        if (!ModelState.IsValid) return Page();

        var agent = new Agent { FullName = Input.FullName, Phone = Input.Phone, Email = Input.Email, AgentCode = Input.AgentCode };
        _db.Agents.Add(agent);

        if (Input.AddCustomer)
        {
            _db.Customers.Add(new Customer
            {
                FullName = Input.CustomerFullName!, Phone = Input.CustomerPhone!, Email = Input.CustomerEmail,
                Address = Input.CustomerAddress, City = Input.CustomerCity, State = Input.CustomerState,
                GstNumber = Input.CustomerGstNumber, Agent = agent
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Agent created.";
        return RedirectToPage("Index");
    }
}
