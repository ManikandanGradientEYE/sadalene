using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

namespace Sadalene.Infrastructure.Services;

public record OtpVerifyResult(bool Success, string? Error, int? UserId, int? CustomerId, int? AgentId);

/// <summary>
/// Generates, sends, and verifies OTPs for Customer/Agent login. Exactly one of userId/customerId/agentId
/// should be set per call, matching OtpLog's "exactly one FK set" convention.
/// </summary>
public class OtpService
{
    private readonly ApplicationDbContext _db;
    private readonly IOtpSender _sender;
    private readonly IConfiguration _config;

    public OtpService(ApplicationDbContext db, IOtpSender sender, IConfiguration config)
    {
        _db = db;
        _sender = sender;
        _config = config;
    }

    public async Task GenerateAndSendAsync(string phone, OtpPurpose purpose,
        int? userId = null, int? customerId = null, int? agentId = null, CancellationToken ct = default)
    {
        var length = int.TryParse(_config["OtpSettings:Length"], out var l) ? l : 6;
        var expiryMinutes = int.TryParse(_config["OtpSettings:ExpiryMinutes"], out var e) ? e : 5;
        var code = GenerateCode(length);

        _db.OtpLogs.Add(new OtpLog
        {
            Phone = phone,
            OtpCode = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            UserId = userId,
            CustomerId = customerId,
            AgentId = agentId
        });
        await _db.SaveChangesAsync(ct);

        await _sender.SendAsync(phone, code, purpose, ct);
    }

    public async Task<OtpVerifyResult> VerifyAsync(string phone, string code, OtpPurpose purpose, CancellationToken ct = default)
    {
        var otp = await _db.OtpLogs
            .Where(o => o.Phone == phone && o.OtpCode == code && o.Purpose == purpose
                && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp == null)
            return new OtpVerifyResult(false, "Invalid or expired OTP.", null, null, null);

        otp.IsUsed = true;
        otp.VerifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OtpVerifyResult(true, null, otp.UserId, otp.CustomerId, otp.AgentId);
    }

    private static string GenerateCode(int length)
    {
        var min = (int)Math.Pow(10, length - 1);
        var max = (int)Math.Pow(10, length) - 1;
        return Random.Shared.Next(min, max + 1).ToString();
    }
}
