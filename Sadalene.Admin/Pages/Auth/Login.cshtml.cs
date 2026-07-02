using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Auth;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public LoginModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Phone == Input.Phone && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid phone number or password.");
            return Page();
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.MobilePhone, user.Phone),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToPage("/Index");
    }
}
