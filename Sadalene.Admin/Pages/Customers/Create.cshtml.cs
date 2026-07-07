using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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

    public class InputModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GstNumber { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Phone == Input.Phone))
        {
            ModelState.AddModelError(string.Empty, "A customer with this phone number already exists.");
            return Page();
        }
        if (!string.IsNullOrWhiteSpace(Input.Email) && await _db.Customers.AnyAsync(x => x.AgentId == null && x.Email == Input.Email))
        {
            ModelState.AddModelError(string.Empty, "A customer with this email already exists.");
            return Page();
        }

        _db.Customers.Add(new Customer
        {
            FullName  = Input.FullName, Phone = Input.Phone, Email = Input.Email,
            Address   = Input.Address,  City  = Input.City,  State = Input.State,
            GstNumber = Input.GstNumber
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer created successfully.";
        return RedirectToPage("Index");
    }
}
