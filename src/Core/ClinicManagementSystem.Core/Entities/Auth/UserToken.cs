using ClinicManagementSystem.Domain.Common;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class UserToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public TokenType TokenType { get; set; }

    public string TokenHash {  get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public string? CreatedByIp { get; set; }


    // ── Navigation Properties ───────────────────────────────────────────
    public virtual ApplicationUser? User { get; set; }
}
