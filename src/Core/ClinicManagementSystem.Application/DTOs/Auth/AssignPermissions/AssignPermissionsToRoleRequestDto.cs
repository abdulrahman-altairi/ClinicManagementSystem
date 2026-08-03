namespace ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;

public sealed class AssignPermissionsToRoleRequestDto
{
    public Guid RoleId { get; init; }
    public List<Guid> PermissionIds { get; init; } = [];
}