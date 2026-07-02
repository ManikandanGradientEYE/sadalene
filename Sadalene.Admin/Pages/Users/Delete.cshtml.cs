using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Users;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) => _db = db;

    public string UserName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        UserName = user.FullName;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null) { user.IsActive = false; user.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        TempData["Success"] = "User deactivated.";
        return RedirectToPage("Index");
    }
}
