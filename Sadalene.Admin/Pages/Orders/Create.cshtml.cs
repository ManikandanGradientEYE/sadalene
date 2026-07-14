using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sadalene.Admin.Services;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Admin.Pages.Orders;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly OrderInventoryService _inventory;
    public CreateModel(ApplicationDbContext db, OrderInventoryService inventory) { _db = db; _inventory = inventory; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Customer> WalkinCustomers { get; set; } = [];
    public List<Agent> Agents { get; set; } = [];

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
            ModelState.AddModelError(string.Empty, "Scan at least one product into the order.");

        if (Input.CustomerId == 0)
            ModelState.AddModelError(string.Empty, "Please select a customer.");

        // Only fetch the handful of products actually referenced by this order — the catalog can run
        // into the tens of thousands, so loading it wholesale here (like the dropdown used to) doesn't scale.
        var productIds = Input.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Include(p => p.UomMaster)
            .Include(p => p.InventoryRecords)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // Group by product before validating: two scans of the same SKU produce two line items,
        // and each must be checked against the combined amount, not independently against the same stock.
        foreach (var group in Input.Items.GroupBy(i => i.ProductId))
        {
            if (!products.TryGetValue(group.Key, out var product))
            {
                ModelState.AddModelError(string.Empty, "One of the scanned products could not be found.");
                continue;
            }

            var totalEffective = group.Sum(i => i.EffectiveQuantity);
            var stock = product.InventoryRecords.Sum(i => i.QuantityAvailable);
            if (totalEffective > stock)
                ModelState.AddModelError(string.Empty,
                    $"Requested quantity for {product.Name} ({totalEffective:N2}) exceeds available stock ({stock:N2}).");
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
            var product = products.GetValueOrDefault(item.ProductId);
            order.Items.Add(new OrderItem
            {
                ProductId           = item.ProductId,
                Product             = product!,
                Quantity            = item.Quantity,
                UnitType            = item.UnitType,
                UnitOfMeasure       = !string.IsNullOrWhiteSpace(item.UnitOfMeasure) ? item.UnitOfMeasure! : (product?.UomMaster?.Name ?? "Units"),
                AddedByBarcodeScan  = item.AddedByBarcodeScan,
                ScannedBarcodeValue = item.AddedByBarcodeScan ? item.ScannedBarcodeValue : null
            });
        }

        _db.Orders.Add(order);

        var adjustedBy = User.FindFirstValue(ClaimTypes.Name) ?? "System";
        _inventory.DeductForOrder(order, adjustedBy);

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
    }
}
