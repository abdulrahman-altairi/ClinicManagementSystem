namespace ClinicManagementSystem.Application.DTOs.Auth.Role;

public class UpdateRoleRequestDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
