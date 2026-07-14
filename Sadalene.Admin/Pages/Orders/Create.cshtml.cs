using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Entities.Products;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Orders;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Customer> WalkinCustomers { get; set; } = [];
    public List<Agent> Agents { get; set; } = [];
    public List<Product> Products { get; set; } = [];

    public class InputModel
    {
        public int? AgentId { get; set; }
        [Required] public int CustomerId { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemInput> Items { get; set; } = [];
    }

    public class OrderItemInput
    {
        [Required] public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        public string? UnitOfMeasure { get; set; }
        public bool AddedByBarcodeScan { get; set; }
        public string? ScannedBarcodeValue { get; set; }
        public OrderItemUnitType UnitType { get; set; } = OrderItemUnitType.Full;

        public decimal EffectiveQuantity => UnitType == OrderItemUnitType.Half ? Quantity * 0.5m : Quantity;
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        Input.Items = Input.Items.Where(i => i.ProductId != 0).ToList();
        if (Input.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one product to the order.");

        if (Input.CustomerId == 0)
            ModelState.AddModelError(string.Empty, "Please select a customer.");

        foreach (var item in Input.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null) continue;

            var stock = product.InventoryRecords.Sum(i => i.QuantityAvailable);
            if (item.EffectiveQuantity > stock)
                ModelState.AddModelError(string.Empty,
                    $"Requested quantity for {product.Name} ({item.Quantity:N2} × {item.UnitType} = {item.EffectiveQuantity:N2}) exceeds available stock ({stock:N2}).");
        }

        if (!ModelState.IsValid) return Page();

        var customer = await _db.Customers.FindAsync(Input.CustomerId);
        if (customer == null)
        {
            ModelState.AddModelError(string.Empty, "Selected customer could not be found.");
            return Page();
        }

        if (Input.AgentId.HasValue && customer.AgentId != Input.AgentId)
        {
            ModelState.AddModelError(string.Empty, "The selected customer does not belong to the selected agent.");
            return Page();
        }

        var year = DateTime.UtcNow.Year;
        var seq = await _db.Orders.CountAsync(o => o.OrderDate.Year == year) + 1;
        var orderNumber = $"ORD-{year}-{seq:D6}";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? placedByUserId = int.TryParse(userIdClaim, out var uid) ? uid : null;

        var order = new Order
        {
            OrderNumber    = orderNumber,
            CustomerId     = Input.CustomerId,
            AgentId        = Input.AgentId,
            PlacedByUserId = placedByUserId,
            Status         = OrderStatus.Pending,
            OrderDate      = DateTime.UtcNow,
            Notes          = Input.Notes
        };

        foreach (var item in Input.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            order.Items.Add(new OrderItem
            {
                ProductId           = item.ProductId,
                Quantity            = item.Quantity,
                UnitType            = item.UnitType,
                UnitOfMeasure       = !string.IsNullOrWhiteSpace(item.UnitOfMeasure) ? item.UnitOfMeasure! : (product?.UomMaster?.Name ?? "Units"),
                AddedByBarcodeScan  = item.AddedByBarcodeScan,
                ScannedBarcodeValue = item.AddedByBarcodeScan ? item.ScannedBarcodeValue : null
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Order {orderNumber} created.";
        return RedirectToPage("Details", new { id = order.Id });
    }

    private async Task LoadDropdownsAsync()
    {
        WalkinCustomers = await _db.Customers
            .Where(c => c.IsActive && c.AgentId == null)
            .OrderBy(c => c.FullName)
            .ToListAsync();

        Agents = await _db.Agents
            .Where(a => a.IsActive)
            .OrderBy(a => a.FullName)
            .ToListAsync();

        Products = await _db.Products
            .Include(p => p.UomMaster)
            .Include(p => p.InventoryRecords)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
