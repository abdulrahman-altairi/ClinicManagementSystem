namespace ClinicManagementSystem.Application.DTOs.Auth.ResetPassword;

public sealed class ConfirmEmailRequestDto
{
    public Guid UserId { get; init; }
    public string Token { get; init; } = string.Empty;
}