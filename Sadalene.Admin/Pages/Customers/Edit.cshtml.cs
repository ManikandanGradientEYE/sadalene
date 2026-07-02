using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        c.FullName = Input.FullName; c.Phone = Input.Phone; c.Email = Input.Email;
        c.Address  = Input.Address;  c.City  = Input.City;  c.State = Input.State;
        c.GstNumber = Input.GstNumber; c.IsActive = Input.IsActive; c.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer updated.";
        return RedirectToPage("Index");
    }
}
