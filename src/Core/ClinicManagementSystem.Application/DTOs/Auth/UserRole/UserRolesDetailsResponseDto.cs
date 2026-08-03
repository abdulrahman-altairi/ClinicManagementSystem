namespace ClinicManagementSystem.Application.DTOs.Auth.UserRole;

public sealed class UserRolesDetailsResponseDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<UserRoleResponseDto> AssignedRoles { get; init; } = [];
}