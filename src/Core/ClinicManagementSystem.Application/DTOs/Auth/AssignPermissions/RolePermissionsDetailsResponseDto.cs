using ClinicManagementSystem.Application.DTOs.Auth.Permissions;

namespace ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;

public sealed class RolePermissionsDetailsResponseDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public IReadOnlyList<PermissionResponseDto> AssignedPermissions { get; init; } = [];
}