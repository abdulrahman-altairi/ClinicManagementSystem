namespace ClinicManagementSystem.Application.DTOs.Auth.ResetPassword;

public sealed class ForgotPasswordRequestDto
{
    public string Email { get; init; } = string.Empty;
}