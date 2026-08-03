namespace ClinicManagementSystem.Application.DTOs.Auth.Permissions;

public sealed class UpdatePermissionRequestDto
{
    public string PermissionName { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}