namespace ClinicManagementSystem.Application.DTOs.Auth.UserAssignments;

public sealed class UserPermissionOverrideItemDto
{
    public Guid PermissionId { get; init; }

    public string GrantType { get; init; } = "GRANT";

    public string? Reason { get; init; }

    public DateTimeOffset? ValidTo { get; init; }
}
