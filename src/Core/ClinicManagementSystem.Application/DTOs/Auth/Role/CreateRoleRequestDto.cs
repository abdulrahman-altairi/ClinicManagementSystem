namespace ClinicManagementSystem.Application.DTOs.Auth.Role;

public sealed class CreateRoleRequestDto
{
    public string RoleName { get; init; } = string.Empty;
    public string? Description { get; init; }
}
