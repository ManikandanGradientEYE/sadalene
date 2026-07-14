using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sadalene.API.DTOs.Auth;
using Sadalene.API.Services;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Services;

namespace Sadalene.API.Controllers.Auth;

/// <summary>
/// Single OTP-based login for all three identity types. Phone numbers are unique across
/// Agents/Walk-in-Customers/Users, so a phone number alone identifies exactly one account — no
/// separate Customer/Agent/Staff login flow is needed. Agent-linked customers don't self-login
/// (their agent orders on their behalf), so they're excluded from this lookup.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly OtpService _otp;
    private readonly TokenService _tokens;

    public AuthController(ApplicationDbContext db, OtpService otp, TokenService tokens)
    {
        _db = db;
        _otp = otp;
        _tokens = tokens;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(SendOtpRequest request)
    {
        var phone = request.Phone.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.IsActive && u.Phone == phone);
        if (user != null)
        {
            await _otp.GenerateAndSendAsync(phone, OtpPurpose.Login, userId: user.Id);
            return Ok(new { message = "OTP sent." });
        }

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.IsActive && a.Phone == phone);
        if (agent != null)
        {
            await _otp.GenerateAndSendAsync(phone, OtpPurpose.Login, agentId: agent.Id);
            return Ok(new { message = "OTP sent." });
        }

        // Agent-linked customers are excluded — their phone numbers aren't guaranteed unique (see
        // CustomerConfiguration) and they don't self-serve login; their agent orders on their behalf.
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.IsActive && c.AgentId == null && c.Phone == phone);
        if (customer != null)
        {
            await _otp.GenerateAndSendAsync(phone, OtpPurpose.Login, customerId: customer.Id);
            return Ok(new { message = "OTP sent." });
        }

        return NotFound(new { message = "No account found with this phone number." });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var phone = request.Phone.Trim();
        var result = await _otp.VerifyAsync(phone, request.Code.Trim(), OtpPurpose.Login);
        if (!result.Success)
            return Unauthorized(new { message = result.Error ?? "Invalid OTP." });

        if (result.UserId is int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.IsActive) return Unauthorized(new { message = "Account not found." });

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _tokens.GenerateToken(user.Id, "Staff", user.Role.ToString(), user.FullName, user.Phone);
            return Ok(new AuthResponse(token, "Staff", user.Id, user.FullName, user.Phone));
        }

        if (result.AgentId is int agentId)
        {
            var agent = await _db.Agents.FindAsync(agentId);
            if (agent == null || !agent.IsActive) return Unauthorized(new { message = "Account not found." });

            agent.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _tokens.GenerateToken(agent.Id, "Agent", "Agent", agent.FullName, agent.Phone);
            return Ok(new AuthResponse(token, "Agent", agent.Id, agent.FullName, agent.Phone));
        }

        if (result.CustomerId is int customerId)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer == null || !customer.IsActive) return Unauthorized(new { message = "Account not found." });

            customer.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _tokens.GenerateToken(customer.Id, "Customer", "Customer", customer.FullName, customer.Phone);
            return Ok(new AuthResponse(token, "Customer", customer.Id, customer.FullName, customer.Phone));
        }

        return Unauthorized(new { message = "Account not found." });
    }
}
