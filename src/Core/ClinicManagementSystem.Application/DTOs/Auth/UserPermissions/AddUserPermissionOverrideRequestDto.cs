using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

public sealed class AddUserPermissionOverrideRequestDto
{
    public Guid UserId { get; init; }
    public Guid PermissionId { get; init; }
    public GrantType GrantType { get; init; } = GrantType.Grant;
    public string? Reason { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
}