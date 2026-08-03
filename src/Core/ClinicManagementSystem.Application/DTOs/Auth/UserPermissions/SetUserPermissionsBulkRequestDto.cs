using ClinicManagementSystem.Domain.Enums;

namespace ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

public sealed class SetUserPermissionsBulkRequestDto
{
    public Guid UserId { get; init; }
    public List<UserPermissionOverrideItemDto> Overrides { get; init; } = [];
}

public sealed class UserPermissionOverrideItemDto
{
    public Guid PermissionId { get; init; }
    public GrantType GrantType { get; init; } = GrantType.Grant;
    public string? Reason { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
}