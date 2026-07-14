using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Users;

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
        public UserRole Role { get; set; } = UserRole.Staff;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Phone must be globally unique across Agents/Walk-in-Customers/Users — those are the identities
        // that log into the mobile app directly. An agent's own customers are exempt (ignored here).
        if (await _db.Users.AnyAsync(u => u.Phone == Input.Phone))
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
        if (!string.IsNullOrWhiteSpace(Input.Email) && await _db.Users.AnyAsync(u => u.Email == Input.Email))
        {
            ModelState.AddModelError(string.Empty, "A user with this email already exists.");
            return Page();
        }

        _db.Users.Add(new User
        {
            FullName     = Input.FullName,
            Phone        = Input.Phone,
            Email        = Input.Email ?? string.Empty,
            Role         = Input.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password)
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "User created successfully.";
        return RedirectToPage("Index");
    }
}
