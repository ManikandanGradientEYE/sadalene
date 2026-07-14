using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.DTOs.Orders;
using Sadalene.API.Extensions;
using Sadalene.API.Services;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Extensions;
using Sadalene.Infrastructure.Services;

namespace Sadalene.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ActingCustomerResolver _resolver;
    private readonly CartLookupService _carts;
    private readonly OrderInventoryService _inventory;

    public OrdersController(ApplicationDbContext db, ActingCustomerResolver resolver, CartLookupService carts, OrderInventoryService inventory)
    {
        _db = db;
        _resolver = resolver;
        _carts = carts;
        _inventory = inventory;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(int? customerId, [FromBody] CheckoutRequest? request)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var cart = await _carts.FindActiveCartAsync(User, forCustomerId.Value);
        if (cart == null || cart.Items.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        // Re-fetch products fresh (live rate + stock) — never trust the cart's DisplayUnitPrice
        // snapshot for money math, since a cart can sit for days before checkout.
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Include(p => p.InventoryRecords)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var group in cart.Items.GroupBy(i => i.ProductId))
        {
            if (!products.TryGetValue(group.Key, out var product))
                return BadRequest(new { message = "One of the items in your cart is no longer available." });

            var totalEffective = group.Sum(i => i.EffectiveQuantity);
            var stock = product.InventoryRecords.Sum(i => i.QuantityAvailable);
            if (totalEffective > stock)
                return Conflict(new
                {
                    message = $"Requested quantity for {product.Name} ({totalEffective:N2}) exceeds available stock ({stock:N2})."
                });
        }

        var customer = await _db.Customers.FindAsync(forCustomerId.Value);
        if (customer == null) return NotFound(new { message = "Customer not found." });

        var identityType = User.GetIdentityType();
        var identityId = User.GetIdentityId();

        var year = DateTime.UtcNow.Year;
        var seq = await _db.Orders.CountAsync(o => o.OrderDate.Year == year) + 1;
        var orderNumber = $"ORD-{year}-{seq:D6}";

        var order = new Order
        {
            OrderNumber    = orderNumber,
            CustomerId     = forCustomerId.Value,
            AgentId        = identityType == "Agent" ? identityId : null,
            PlacedByUserId = identityType == "Staff" ? identityId : null,
            Status         = OrderStatus.Pending,
            OrderDate      = DateTime.UtcNow,
            Notes          = request?.Notes
        };

        foreach (var cartItem in cart.Items)
        {
            var product = products[cartItem.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId           = cartItem.ProductId,
                Product             = product,
                Quantity            = cartItem.Quantity,
                UnitType            = cartItem.UnitType,
                UnitPrice           = product.Rate ?? 0,
                UnitOfMeasure       = cartItem.UnitOfMeasure,
                AddedByBarcodeScan  = cartItem.AddedByBarcodeScan,
                ScannedBarcodeValue = cartItem.ScannedBarcodeValue
            });
        }

        _db.Orders.Add(order);

        var adjustedBy = identityType switch
        {
            "Staff" => User.FindFirstValue(ClaimTypes.Name) ?? "Staff",
            "Agent" => $"Agent: {User.FindFirstValue(ClaimTypes.Name)}",
            _       => customer.FullName
        };
        _inventory.DeductForOrder(order, adjustedBy);

        // Leave the cart's items in place as a historical record — marking it CheckedOut (rather than
        // clearing it) means it drops out of the "Active" cart lookup, so the next add-to-cart call
        // naturally creates a fresh cart.
        cart.Status = CartStatus.CheckedOut;

        await _db.SaveChangesAsync();

        return Ok(await BuildDetailAsync(order.Id));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyOrders(int page = 1, int pageSize = 20)
    {
        var identityType = User.GetIdentityType();
        var identityId = User.GetIdentityId();

        var query = _db.Orders.Include(o => o.Customer).Include(o => o.Items).AsQueryable();

        query = identityType switch
        {
            "Customer" => query.Where(o => o.CustomerId == identityId),
            "Agent"    => query.Where(o => o.AgentId == identityId),
            "Staff"    => query.Where(o => o.PlacedByUserId == identityId),
            _          => query.Where(o => false)
        };

        var projected = query.OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto(
                o.Id, o.OrderNumber, o.Customer.FullName, o.OrderDate, o.Status.ToString(),
                o.Items.Count, o.Items.Sum(i => i.UnitPrice * (i.UnitType == OrderItemUnitType.Half ? i.Quantity * 0.5m : i.Quantity))
            ));

        var result = await projected.ToPagedResultAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        var identityType = User.GetIdentityType();
        var identityId = User.GetIdentityId();

        var owns = identityType switch
        {
            "Customer" => order.CustomerId == identityId,
            "Agent"    => order.AgentId == identityId,
            "Staff"    => order.PlacedByUserId == identityId,
            _          => false
        };
        if (!owns) return NotFound();

        return Ok(await BuildDetailAsync(id));
    }

    private async Task<OrderDetailDto?> BuildDetailAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Agent)
            .Include(o => o.PlacedByUser)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return null;

        var items = order.Items.Select(i => new OrderItemDto(
            i.ProductId, i.Product.Name, i.Product.ProductCode,
            i.Quantity, i.UnitType.ToString(), i.UnitOfMeasure, i.EffectiveQuantity, i.UnitPrice, i.LineTotal
        )).ToList();

        return new OrderDetailDto(
            order.Id, order.OrderNumber, order.Customer.FullName, order.Agent?.FullName, order.PlacedByUser?.FullName,
            order.OrderDate, order.Status.ToString(), order.Notes, items, order.GrandTotal
        );
    }
}
