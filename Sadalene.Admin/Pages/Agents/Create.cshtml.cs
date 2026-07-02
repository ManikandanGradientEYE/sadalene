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
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        _db.Agents.Add(new Agent { FullName = Input.FullName, Phone = Input.Phone, Email = Input.Email, AgentCode = Input.AgentCode });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Agent created.";
        return RedirectToPage("Index");
    }
}
