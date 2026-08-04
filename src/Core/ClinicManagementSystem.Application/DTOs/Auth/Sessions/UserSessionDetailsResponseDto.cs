namespace ClinicManagementSystem.Application.DTOs.Auth.Sessions;

public sealed class UserSessionDetailsResponseDto
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public string? DeviceInfo { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset IssuedAtUtc { get; init; }
    public DateTimeOffset ExpiresAtUtc { get; init; }
    public DateTimeOffset? RevokedAtUtc { get; init; }
    public bool IsRevoked { get; init; }
    public string? ReplacedByToken { get; init; }
}