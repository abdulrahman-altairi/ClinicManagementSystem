namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public sealed class LoginRequestDto
{
    public string Identifier { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public string? TwoFactorCode { get; set; }
}
