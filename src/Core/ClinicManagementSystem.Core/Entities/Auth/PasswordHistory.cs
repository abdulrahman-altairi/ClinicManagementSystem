using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class PasswordHistory : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset ChangedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? ChangedByIp { get; set; }
}
