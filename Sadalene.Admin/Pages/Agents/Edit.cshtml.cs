using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Agents;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public NewCustomerInputModel NewCustomer { get; set; } = new();

    public List<Customer> Customers { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        public string? AgentCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class NewCustomerInputModel
    {
        public int AgentId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? GstNumber { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var a = await _db.Agents.Include(x => x.Customers).FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();
        Input = new InputModel { Id = a.Id, FullName = a.FullName, Phone = a.Phone, Email = a.Email, AgentCode = a.AgentCode, IsActive = a.IsActive };
        Customers = a.Customers.OrderBy(c => c.FullName).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ModelState.ClearValidationState(nameof(NewCustomer));
        if (!ModelState.IsValid) return await ReloadAsync(Input.Id);

        if (await _db.Agents.AnyAsync(x => x.Phone == Input.Phone && x.Id != Input.Id))
        {
            ModelState.AddModelError(string.Empty, "An agent with this phone number already exists.");
            return await ReloadAsync(Input.Id);
        }
        if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Phone == Input.Phone))
        {
            ModelState.AddModelError(string.Empty, "A customer with this phone number already exists.");
            return await ReloadAsync(Input.Id);
        }
        if (!string.IsNullOrWhiteSpace(Input.Email))
        {
            if (await _db.Agents.AnyAsync(x => x.Email == Input.Email && x.Id != Input.Id))
            {
                ModelState.AddModelError(string.Empty, "An agent with this email already exists.");
                return await ReloadAsync(Input.Id);
            }
            if (await _db.Customers.AnyAsync(x => x.AgentId == null && x.Email == Input.Email))
            {
                ModelState.AddModelError(string.Empty, "A customer with this email already exists.");
                return await ReloadAsync(Input.Id);
            }
        }

        var a = await _db.Agents.FindAsync(Input.Id);
        if (a == null) return NotFound();
        a.FullName = Input.FullName; a.Phone = Input.Phone; a.Email = Input.Email;
        a.AgentCode = Input.AgentCode; a.IsActive = Input.IsActive; a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Agent updated.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostAddCustomerAsync()
    {
        ModelState.ClearValidationState(nameof(Input));
        if (string.IsNullOrWhiteSpace(NewCustomer.FullName))
            ModelState.AddModelError("NewCustomer.FullName", "Customer name is required.");
        if (string.IsNullOrWhiteSpace(NewCustomer.Phone))
            ModelState.AddModelError("NewCustomer.Phone", "Customer phone is required.");
        if (!ModelState.IsValid) return await ReloadAsync(NewCustomer.AgentId);

        var agent = await _db.Agents.FindAsync(NewCustomer.AgentId);
        if (agent == null) return NotFound();

        _db.Customers.Add(new Customer
        {
            FullName = NewCustomer.FullName!, Phone = NewCustomer.Phone!, Email = NewCustomer.Email,
            Address = NewCustomer.Address, City = NewCustomer.City, State = NewCustomer.State,
            GstNumber = NewCustomer.GstNumber, Agent = agent
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer added to agent.";
        return RedirectToPage(new { id = NewCustomer.AgentId });
    }

    private async Task<IActionResult> ReloadAsync(int agentId)
    {
        var a = await _db.Agents.Include(x => x.Customers).FirstOrDefaultAsync(x => x.Id == agentId);
        if (a == null) return NotFound();
        if (Input.Id == 0)
            Input = new InputModel { Id = a.Id, FullName = a.FullName, Phone = a.Phone, Email = a.Email, AgentCode = a.AgentCode, IsActive = a.IsActive };
        Customers = a.Customers.OrderBy(c => c.FullName).ToList();
        return Page();
    }
}
