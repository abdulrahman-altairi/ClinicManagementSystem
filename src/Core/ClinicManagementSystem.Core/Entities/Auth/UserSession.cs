using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class UserSession : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset IssuedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public bool IsRevoked { get; set; } = false;

    public string? ReplacedByToken { get; set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAtUtc <= now;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);
}
