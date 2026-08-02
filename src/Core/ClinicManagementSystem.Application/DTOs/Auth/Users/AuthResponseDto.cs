namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public sealed class AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset AccessTokenExpiration { get; init; }
}
