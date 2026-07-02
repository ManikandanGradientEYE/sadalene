using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Agents;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) => _db = db;
    public string AgentName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var a = await _db.Agents.FindAsync(id);
        if (a == null) return NotFound();
        AgentName = a.FullName;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var a = await _db.Agents.FindAsync(id);
        if (a != null) { a.IsActive = false; a.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Agent deactivated.";
        return RedirectToPage("Index");
    }
}
