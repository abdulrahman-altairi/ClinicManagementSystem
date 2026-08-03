namespace ClinicManagementSystem.Application.DTOs.Auth.UserPermissions;

public sealed class UserPermissionOverridesDetailsResponseDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public IReadOnlyList<UserPermissionResponseDto> Overrides { get; init; } = [];
}