using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Users;

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
        public UserRole Role { get; set; }
        public string? NewPassword { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        Input = new InputModel
        {
            Id = user.Id, FullName = user.FullName, Phone = user.Phone,
            Email = user.Email, Role = user.Role, IsActive = user.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Phone must be globally unique across Agents/Walk-in-Customers/Users — those are the identities
        // that log into the mobile app directly. An agent's own customers are exempt (ignored here).
        if (await _db.Users.AnyAsync(u => u.Phone == Input.Phone && u.Id != Input.Id))
        {
            ModelState.AddModelError(string.Empty, "A user with this phone number already exists.");
            return Page();
        }
        if (await _db.Agents.AnyAsync(a => a.Phone == Input.Phone))
        {
            ModelState.AddModelError(string.Empty, "An agent with this phone number already exists.");
            return Page();
        }
        if (await _db.Customers.AnyAsync(c => c.AgentId == null && c.Phone == Input.Phone))
        {
            ModelState.AddModelError(string.Empty, "A customer with this phone number already exists.");
            return Page();
        }
        if (!string.IsNullOrWhiteSpace(Input.Email) && await _db.Users.AnyAsync(u => u.Email == Input.Email && u.Id != Input.Id))
        {
            ModelState.AddModelError(string.Empty, "A user with this email already exists.");
            return Page();
        }

        var user = await _db.Users.FindAsync(Input.Id);
        if (user == null) return NotFound();

        user.FullName  = Input.FullName;
        user.Phone     = Input.Phone;
        user.Email     = Input.Email ?? string.Empty;
        user.Role      = Input.Role;
        user.IsActive  = Input.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);

        await _db.SaveChangesAsync();
        TempData["Success"] = "User updated successfully.";
        return RedirectToPage("Index");
    }
}
