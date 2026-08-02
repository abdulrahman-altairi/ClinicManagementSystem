using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class UserRole : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ValidTo { get; set; }

    public Guid? AssignedBy { get; set; }

    public bool IsActive(DateTimeOffset now)
        => ValidFrom <= now && (ValidTo == null || ValidTo > now);
}
