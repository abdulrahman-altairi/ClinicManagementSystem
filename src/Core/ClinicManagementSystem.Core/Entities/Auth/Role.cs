using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class Role : BaseEntity<Guid>
{
    public string RoleName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;
    public bool IsActive { get; set; } = true;


    // ── Navigation Properties ───────────────────────────────────────────

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
