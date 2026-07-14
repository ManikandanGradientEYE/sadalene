using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.DTOs.Cart;
using Sadalene.API.Extensions;
using Sadalene.API.Services;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ActingCustomerResolver _resolver;
    private readonly CartLookupService _carts;

    public CartController(ApplicationDbContext db, ActingCustomerResolver resolver, CartLookupService carts)
    {
        _db = db;
        _resolver = resolver;
        _carts = carts;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(int? customerId)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var cart = await _carts.FindActiveCartAsync(User, forCustomerId.Value);
        return Ok(cart == null ? EmptyCart(forCustomerId.Value) : MapCart(cart));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request, int? customerId)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var product = await _db.Products.Include(p => p.UomMaster)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
        if (product == null) return NotFound(new { message = "Product not found." });

        if (!Enum.TryParse<OrderItemUnitType>(request.UnitType, ignoreCase: true, out var unitType))
            unitType = OrderItemUnitType.Full;

        if (unitType == OrderItemUnitType.Half && !(product.UomMaster?.AllowsHalfUnit ?? false))
            return BadRequest(new { message = "This product cannot be ordered as a Half unit." });

        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0." });

        var cart = await _carts.GetOrCreateActiveCartAsync(User, forCustomerId.Value);

        // The single-division rule: reject as soon as a second division is attempted, before touching the cart.
        if (cart.DivisionId.HasValue && cart.DivisionId != product.DivisionId)
        {
            var currentDivisionName = await _db.Divisions.Where(d => d.Id == cart.DivisionId).Select(d => d.Name).FirstOrDefaultAsync();
            var newDivisionName = await _db.Divisions.Where(d => d.Id == product.DivisionId).Select(d => d.Name).FirstOrDefaultAsync();
            return Conflict(new
            {
                message = $"This cart already contains items from the {currentDivisionName} division. " +
                          $"Start a new order to add items from {newDivisionName}."
            });
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == product.Id && i.UnitType == unitType);
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId              = cart.Id,
                ProductId           = product.Id,
                Quantity            = request.Quantity,
                UnitType            = unitType,
                UnitOfMeasure       = product.UomMaster?.Name ?? "Units",
                DisplayUnitPrice    = product.Rate ?? 0,
                AddedByBarcodeScan  = request.AddedByBarcodeScan,
                ScannedBarcodeValue = request.AddedByBarcodeScan ? request.ScannedBarcodeValue : null
            });
        }

        cart.DivisionId ??= product.DivisionId;
        TouchStaffModifier(cart);

        await _db.SaveChangesAsync();
        return Ok(MapCart(cart));
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartItemRequest request, int? customerId)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var cart = await GetOwnedCartWithItemAsync(forCustomerId.Value, cartItemId);
        if (cart == null) return NotFound();

        var item = cart.Items.First(i => i.Id == cartItemId);

        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0." });

        if (!Enum.TryParse<OrderItemUnitType>(request.UnitType, ignoreCase: true, out var unitType))
            unitType = item.UnitType;

        if (unitType == OrderItemUnitType.Half && !(item.Product.UomMaster?.AllowsHalfUnit ?? false))
            return BadRequest(new { message = "This product cannot be ordered as a Half unit." });

        item.Quantity = request.Quantity;
        item.UnitType = unitType;
        TouchStaffModifier(cart);

        await _db.SaveChangesAsync();
        return Ok(MapCart(cart));
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, int? customerId)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var cart = await GetOwnedCartWithItemAsync(forCustomerId.Value, cartItemId);
        if (cart == null) return NotFound();

        var item = cart.Items.First(i => i.Id == cartItemId);
        cart.Items.Remove(item);
        _db.CartItems.Remove(item);

        if (cart.Items.Count == 0) cart.DivisionId = null;
        TouchStaffModifier(cart);

        await _db.SaveChangesAsync();
        return Ok(MapCart(cart));
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(int? customerId)
    {
        var (forCustomerId, error) = await _resolver.ResolveAsync(User, customerId);
        if (forCustomerId == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = error });

        var cart = await _carts.FindActiveCartAsync(User, forCustomerId.Value);
        if (cart == null) return Ok(EmptyCart(forCustomerId.Value));

        _db.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        cart.DivisionId = null;
        TouchStaffModifier(cart);

        await _db.SaveChangesAsync();
        return Ok(MapCart(cart));
    }

    // --- helpers ---

    private async Task<Cart?> GetOwnedCartWithItemAsync(int forCustomerId, int cartItemId)
    {
        var cart = await _carts.FindActiveCartAsync(User, forCustomerId);
        return cart != null && cart.Items.Any(i => i.Id == cartItemId) ? cart : null;
    }

    private void TouchStaffModifier(Cart cart)
    {
        if (User.GetIdentityType() == "Staff") cart.LastModifiedByUserId = User.GetIdentityId();
    }

    private static CartDto EmptyCart(int forCustomerId) => new(0, forCustomerId, null, null, "Active", [], 0);

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.Id, i.ProductId, i.Product.Name, i.Product.ProductCode,
            i.Quantity, i.UnitType.ToString(), i.UnitOfMeasure, i.EffectiveQuantity,
            i.DisplayUnitPrice, i.DisplayUnitPrice * i.EffectiveQuantity, i.AddedByBarcodeScan
        )).ToList();

        return new CartDto(
            cart.Id, cart.ForCustomerId, cart.DivisionId, cart.Division?.Name,
            cart.Status.ToString(), items, items.Sum(i => i.LineTotal)
        );
    }
}
