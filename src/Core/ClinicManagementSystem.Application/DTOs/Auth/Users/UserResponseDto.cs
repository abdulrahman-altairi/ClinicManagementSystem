namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public sealed class UserResponseDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool PhoneVerified { get; init; }
    public bool EmailVerified { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public bool IsActive { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTimeOffset? LastLoginUtc { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<string> Roles { get; init; } = new();
    public AuthResponseDto? AuthResponseDto { get; init; }
}
