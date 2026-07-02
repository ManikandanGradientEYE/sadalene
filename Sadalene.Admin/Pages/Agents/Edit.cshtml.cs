using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Agents;

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
        public string? AgentCode { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var a = await _db.Agents.FindAsync(id);
        if (a == null) return NotFound();
        Input = new InputModel { Id = a.Id, FullName = a.FullName, Phone = a.Phone, Email = a.Email, AgentCode = a.AgentCode, IsActive = a.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var a = await _db.Agents.FindAsync(Input.Id);
        if (a == null) return NotFound();
        a.FullName = Input.FullName; a.Phone = Input.Phone; a.Email = Input.Email;
        a.AgentCode = Input.AgentCode; a.IsActive = Input.IsActive; a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Agent updated.";
        return RedirectToPage("Index");
    }
}
