using Sadalene.Core.Enums;

namespace Sadalene.Core.Entities.Auth;

/// <summary>
/// Tracks OTP delivery and verification for all login types.
/// </summary>
public class OtpLog
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }

    // Nullable FK — OTP is linked to exactly one login type
    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }
}
