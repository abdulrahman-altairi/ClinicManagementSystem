namespace ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;

public sealed class AddPermissionToRoleDto
{
    public Guid RoleId { get; init; }
    public Guid PermissionId { get; init; }
}