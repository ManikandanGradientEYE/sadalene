using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sadalene.API.Services;

/// <summary>
/// Issues JWTs for all three mobile identity types (Customer/Agent/Staff). The three tables are
/// independently scoped (a Customer.Id=5 and Agent.Id=5 are unrelated rows), so every token carries
/// a custom "identityType" claim alongside NameIdentifier — callers must always read both together.
/// </summary>
public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string GenerateToken(int id, string identityType, string role, string displayName, string phone)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new("identityType", identityType),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.MobilePhone, phone)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
