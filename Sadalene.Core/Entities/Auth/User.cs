using Sadalene.Core.Common;
using Sadalene.Core.Enums;

namespace Sadalene.Core.Entities.Auth;

/// <summary>
/// Internal staff and admin users who manage the backend system.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<OtpLog> OtpLogs { get; set; } = [];
}
