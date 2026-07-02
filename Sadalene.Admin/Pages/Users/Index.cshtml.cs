using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<User> Users { get; set; } = [];

    public async Task OnGetAsync() =>
        Users = await _db.Users.OrderBy(u => u.FullName).ToListAsync();
}
