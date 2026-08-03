namespace ClinicManagementSystem.Application.DTOs.Auth.UserRole;

public sealed class AssignRolesToUserRequestDto
{
    public Guid UserId { get; init; }
    public List<UserRoleAssignmentDto> Roles { get; init; } = [];
}

public sealed class UserRoleAssignmentDto
{
    public Guid RoleId { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
}