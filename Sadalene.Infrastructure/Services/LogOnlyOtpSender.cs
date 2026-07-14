using Microsoft.Extensions.Logging;
using Sadalene.Core.Enums;

namespace Sadalene.Infrastructure.Services;

/// <summary>
/// Development-only placeholder — logs the OTP instead of sending an SMS. No SMS gateway has been
/// chosen yet; swap in a real IOtpSender implementation (e.g. MSG91, Twilio) before going live.
/// </summary>
public class LogOnlyOtpSender : IOtpSender
{
    private readonly ILogger<LogOnlyOtpSender> _logger;
    public LogOnlyOtpSender(ILogger<LogOnlyOtpSender> logger) => _logger = logger;

    public Task SendAsync(string phone, string otpCode, OtpPurpose purpose, CancellationToken ct = default)
    {
        _logger.LogInformation("OTP for {Phone} ({Purpose}): {OtpCode}", phone, purpose, otpCode);
        return Task.CompletedTask;
    }
}
