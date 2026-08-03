using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

public sealed class UserPermissionResponseDto
{
    public Guid UserPermissionId { get; init; }
    public Guid UserId { get; init; }
    public Guid PermissionId { get; init; }
    public string PermissionCode { get; init; } = string.Empty;
    public string PermissionName { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public GrantType GrantType { get; init; } = GrantType.Grant;
    public string? Reason { get; init; }
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public Guid? GrantedBy { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}