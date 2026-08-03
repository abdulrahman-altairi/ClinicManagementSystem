using ClinicManagementSystem.Application.DTOs.Auth.Permissions;

namespace ClinicManagementSystem.Application.DTOs.Auth.Role;

public sealed class RoleResponseDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsSystemRole { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public List<PermissionResponseDto> Permissions { get; init; } = new();
}
