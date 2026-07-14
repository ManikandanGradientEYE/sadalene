using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.Extensions;
using Sadalene.Infrastructure.Data;

namespace Sadalene.API.Services;

/// <summary>
/// Resolves which Customer a Cart/Order request is acting for, and enforces who's allowed to act for
/// whom. This is the one place an authorization bypass would matter most (an Agent acting as an
/// arbitrary customer) — CartController and OrdersController both funnel through here rather than
/// each re-implementing the check.
/// </summary>
public class ActingCustomerResolver
{
    private readonly ApplicationDbContext _db;
    public ActingCustomerResolver(ApplicationDbContext db) => _db = db;

    public async Task<(int? CustomerId, string? Error)> ResolveAsync(ClaimsPrincipal user, int? customerId)
    {
        var identityType = user.GetIdentityType();
        var identityId = user.GetIdentityId();

        switch (identityType)
        {
            case "Customer":
                return (identityId, null);

            case "Agent":
                if (customerId is null)
                    return (null, "customerId is required.");
                var belongsToAgent = await _db.Customers
                    .AnyAsync(c => c.Id == customerId && c.AgentId == identityId && c.IsActive);
                return belongsToAgent ? (customerId, null) : (null, "This customer does not belong to you.");

            case "Staff":
                if (customerId is null)
                    return (null, "customerId is required.");
                var exists = await _db.Customers.AnyAsync(c => c.Id == customerId && c.IsActive);
                return exists ? (customerId, null) : (null, "Customer not found.");

            default:
                return (null, "Unknown identity type.");
        }
    }
}
