namespace ClinicManagementSystem.Application.DTOs.Auth.Sessions;

public sealed class UserSessionResponseDto
{
    public Guid SessionId { get; init; }
    public string? DeviceInfo { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset IssuedAtUtc { get; init; }
    public DateTimeOffset ExpiresAtUtc { get; init; }
    public bool IsActive { get; init; }
}