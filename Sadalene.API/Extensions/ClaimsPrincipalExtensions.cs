using System.Security.Claims;

namespace Sadalene.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>"Customer" | "Agent" | "Staff" — always read together with GetIdentityId(),
    /// since the three tables have independently-scoped Ids.</summary>
    public static string GetIdentityType(this ClaimsPrincipal user) =>
        user.FindFirstValue("identityType") ?? string.Empty;

    public static int GetIdentityId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
