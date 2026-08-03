namespace ClinicManagementSystem.Application.DTOs.Auth.UserRole;

public sealed class AddRoleToUserRequestDto
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
}