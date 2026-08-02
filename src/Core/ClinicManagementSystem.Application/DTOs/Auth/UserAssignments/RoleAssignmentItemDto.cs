namespace ClinicManagementSystem.Application.DTOs.Auth.UserAssignments;

public sealed class RoleAssignmentItemDto
{
    public string RoleName { get; init; } = string.Empty;

    public DateTimeOffset? ValidTo { get; init; }
}
