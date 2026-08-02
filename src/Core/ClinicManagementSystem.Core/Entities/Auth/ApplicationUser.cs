using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class ApplicationUser : AggregateRoot<Guid>
{
    // ─── Identity ────────────────────────────────────────────────────────────
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;

    // ─── Credentials ─────────────────────────────────────────────────────────
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    // ─── Profile ─────────────────────────────────────────────────────────────
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName => $"{FirstName} {LastName}".Trim();
    public string? PhoneNumber { get; set; }
    public bool PhoneVerified { get; set; } = false;
    public bool EmailVerified { get; set; } = false;
    public string? AvatarUrl { get; set; }

    // ─── Two-Factor Auth ─────────────────────────────────────────────────────
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }

    // ─── Lockout ─────────────────────────────────────────────────────────────
    public bool LockoutEnabled { get; set; } = true;
    public DateTimeOffset? LockoutEnd { get; set; }
    public byte AccessFailedCount { get; set; } = 0;

    // ─── Audit / Login Tracking ───────────────────────────────────────────────
    public DateTimeOffset? LastLoginUtc { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTimeOffset? PasswordChangedUtc { get; set; }

    // ─── Status ──────────────────────────────────────────────────────────────
    public bool IsActive { get; set; } = true;

    // ─── Domain Behaviour ────────────────────────────────────────────────────

    public bool IsLockedOut(DateTimeOffset now)
        => LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > now;


    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration, DateTimeOffset now)
    {
        if (!LockoutEnabled) return;

        AccessFailedCount++;
        if (AccessFailedCount >= maxAttempts)
        {
            LockoutEnd = now.Add(lockoutDuration);
            AccessFailedCount = 0; 
        }
    }

    public void RecordSuccessfulLogin(DateTimeOffset now, string? ipAddress)
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        LastLoginUtc = now;
        LastLoginIp = ipAddress;
    }
}
