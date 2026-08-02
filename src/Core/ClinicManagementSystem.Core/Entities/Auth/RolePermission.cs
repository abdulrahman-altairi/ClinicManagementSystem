using ClinicManagementSystem.Domain.Common;

namespace ClinicManagementSystem.Domain.Entities.Auth;

public class RolePermission : BaseEntity<Guid>
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
