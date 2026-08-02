namespace ClinicManagementSystem.Application.DTOs.Auth.Sessions;

public sealed class RefreshTokenRequestDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
