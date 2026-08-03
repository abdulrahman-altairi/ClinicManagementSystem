namespace ClinicManagementSystem.Application.DTOs.Auth.AssignPermissions;

public sealed class RolePermissionResponseDto
{
    public Guid RolePermissionId { get; init; }
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public Guid PermissionId { get; init; }
    public string PermissionCode { get; init; } = string.Empty;
    public string PermissionName { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}