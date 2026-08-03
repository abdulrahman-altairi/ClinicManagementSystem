namespace ClinicManagementSystem.Application.DTOs.Auth.Permissions;

public sealed class CreatePermissionRequestDto
{
    public string PermissionCode { get; init; } = string.Empty;
    public string PermissionName { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string? Description { get; init; }
}