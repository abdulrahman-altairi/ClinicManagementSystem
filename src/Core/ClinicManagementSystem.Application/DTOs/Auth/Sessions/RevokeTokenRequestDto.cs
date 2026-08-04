namespace ClinicManagementSystem.Application.DTOs.Auth.Sessions;

public sealed class RevokeTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}