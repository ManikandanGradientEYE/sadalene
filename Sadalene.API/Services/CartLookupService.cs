using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.Extensions;
using Sadalene.Core.Entities.Orders;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.API.Services;

/// <summary>
/// Resolves the caller's active Cart for a given ForCustomerId, using one consistent ownership-shape
/// rule set (Customer: their own cart; Agent: AgentId+ForCustomerId; Staff: shared cart, identified by
/// neither CustomerId nor AgentId being set). Shared by CartController and OrdersController so this
/// logic lives in exactly one place.
/// </summary>
public class CartLookupService
{
    private readonly ApplicationDbContext _db;
    public CartLookupService(ApplicationDbContext db) => _db = db;

    public IQueryable<Cart> ActiveCartQuery(ClaimsPrincipal user, int forCustomerId)
    {
        var identityType = user.GetIdentityType();
        var identityId = user.GetIdentityId();

        IQueryable<Cart> query = _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.Division)
            .Where(c => c.Status == CartStatus.Active);

        return identityType switch
        {
            "Customer" => query.Where(c => c.CustomerId == identityId),
            "Agent"    => query.Where(c => c.AgentId == identityId && c.ForCustomerId == forCustomerId),
            _          => query.Where(c => c.CustomerId == null && c.AgentId == null && c.ForCustomerId == forCustomerId)
        };
    }

    public Task<Cart?> FindActiveCartAsync(ClaimsPrincipal user, int forCustomerId) =>
        ActiveCartQuery(user, forCustomerId).FirstOrDefaultAsync();

    public async Task<Cart> GetOrCreateActiveCartAsync(ClaimsPrincipal user, int forCustomerId)
    {
        var cart = await FindActiveCartAsync(user, forCustomerId);
        if (cart != null) return cart;

        var identityType = user.GetIdentityType();
        var identityId = user.GetIdentityId();

        cart = new Cart
        {
            CustomerId    = identityType == "Customer" ? identityId : null,
            AgentId       = identityType == "Agent" ? identityId : null,
            ForCustomerId = forCustomerId,
            Status        = CartStatus.Active
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }
}
