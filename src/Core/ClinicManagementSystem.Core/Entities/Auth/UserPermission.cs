using ClinicManagementSystem.Domain.Common;
using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class UserPermission : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public Guid PermissionId { get; set; }

    public GrantType GrantType { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ValidTo { get; set; }

    public Guid? GrantedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsEffective(DateTimeOffset now)
        => IsActive && ValidFrom <= now && (ValidTo == null || ValidTo > now);
}
