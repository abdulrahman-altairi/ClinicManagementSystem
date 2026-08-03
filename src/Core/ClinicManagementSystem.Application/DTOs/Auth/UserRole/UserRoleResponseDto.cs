namespace ClinicManagementSystem.Application.DTOs.Auth.UserRole;

public sealed class UserRoleResponseDto
{
    public Guid UserRoleId { get; init; }
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public bool IsActive { get; init; }
    public Guid? AssignedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}