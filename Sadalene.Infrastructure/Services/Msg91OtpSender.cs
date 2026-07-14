using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sadalene.Core.Enums;

namespace Sadalene.Infrastructure.Services;

/// <summary>
/// Sends OTPs via MSG91's "Send OTP" API (https://control.msg91.com/api/v5/otp), passing our own
/// pre-generated code via the `otp` query param — MSG91 just delivers it as SMS using the configured
/// template, we still verify locally against OtpLog (see OtpService), so no MSG91-side verify call
/// is needed. Requires Msg91Settings:AuthKey and Msg91Settings:TemplateId in configuration.
/// </summary>
public class Msg91OtpSender : IOtpSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<Msg91OtpSender> _logger;

    public Msg91OtpSender(HttpClient http, IConfiguration config, ILogger<Msg91OtpSender> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string phone, string otpCode, OtpPurpose purpose, CancellationToken ct = default)
    {
        var authKey = _config["Msg91Settings:AuthKey"];
        var templateId = _config["Msg91Settings:TemplateId"];

        if (string.IsNullOrWhiteSpace(authKey) || string.IsNullOrWhiteSpace(templateId))
            throw new InvalidOperationException(
                "Msg91Settings:AuthKey and Msg91Settings:TemplateId must be configured before OTPs can be sent via MSG91.");

        var mobile = ToMsg91Mobile(phone);
        var url = "https://control.msg91.com/api/v5/otp" +
                  $"?template_id={Uri.EscapeDataString(templateId)}" +
                  $"&mobile={Uri.EscapeDataString(mobile)}" +
                  $"&authkey={Uri.EscapeDataString(authKey)}" +
                  $"&otp={Uri.EscapeDataString(otpCode)}";

        var response = await _http.GetAsync(url, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("MSG91 OTP send failed for {Phone}: {StatusCode} {Body}", phone, response.StatusCode, body);
            throw new InvalidOperationException("Failed to send OTP via MSG91.");
        }
    }

    // MSG91 expects the country code prefixed with no '+' (e.g. 91XXXXXXXXXX). Assumes India (91) for
    // bare 10-digit numbers; numbers that already include a country code prefix pass through unchanged.
    private static string ToMsg91Mobile(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 10 ? "91" + digits : digits;
    }
}
