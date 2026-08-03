using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

public sealed class UpdateUserPermissionOverrideRequestDto
{
    public GrantType GrantType { get; init; } = GrantType.Grant;
    public string? Reason { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public bool IsActive { get; init; } = true;
}