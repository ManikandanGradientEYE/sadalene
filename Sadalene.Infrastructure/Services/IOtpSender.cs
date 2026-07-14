using Sadalene.Core.Enums;

namespace Sadalene.Infrastructure.Services;

public interface IOtpSender
{
    Task SendAsync(string phone, string otpCode, OtpPurpose purpose, CancellationToken ct = default);
}
