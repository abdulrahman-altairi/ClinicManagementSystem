namespace ClinicManagementSystem.Application.DTOs.Auth.Permissions;

public sealed class GroupedPermissionsResponseDto
{
    public string Module { get; init; } = string.Empty;
    public IReadOnlyList<PermissionResponseDto> Permissions { get; init; } = [];
}